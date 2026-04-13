using boardomapi.Models;
using boardomapi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.DeviceController;

public partial class DeviceController
{
  [HttpPost("add")]
  public async Task<IActionResult> AddDeviceAsync([FromBody] AddDeviceRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.DeviceId) || string.IsNullOrWhiteSpace(request.FriendlyName))
      return BadRequest(new { error = "DeviceId and FriendlyName are required" });

    var existing = await _db.Devices
      .IgnoreQueryFilters()
      .FirstOrDefaultAsync(d => d.DeviceId == request.DeviceId && d.UserId == GetUserId());

    if (existing != null)
    {
      if (existing.IsDeleted)
        existing.IsDeleted = false;

      existing.FriendlyName = request.FriendlyName;
      await _db.SaveChangesAsync();
      return Ok(new { message = "Device updated", deviceId = existing.DeviceId, friendlyName = existing.FriendlyName });
    }

    var device = new Device
    {
      DeviceId = request.DeviceId,
      FriendlyName = request.FriendlyName,
      UserId = GetUserId()
    };

    _db.Devices.Add(device);
    await _db.SaveChangesAsync();

    return Created($"/device/{device.DeviceId}", new { message = "Device registered", deviceId = device.DeviceId, friendlyName = device.FriendlyName });
  }

  [AllowAnonymous]
  [HttpPost("heartbeat")]
  public async Task<IActionResult> HeartbeatAsync(
    [FromBody] DeviceHeartbeatRequest request,
    [FromServices] Auth0TokenService tokenService)
  {
    var (device, error) = await FindDeviceOrErrorAsync(request.DeviceId);
    if (error != null)
      return error;

    device!.LastHeartbeat = DateTime.UtcNow;

    string? accessToken = null;
    string? newRefreshToken = null;

    // If device sent a refresh token, use it; otherwise fall back to client_credentials
    if (!string.IsNullOrWhiteSpace(request.RefreshToken))
    {
      var tokenResult = await tokenService.RefreshAccessTokenAsync(request.RefreshToken);
      if (tokenResult.HasValue)
      {
        accessToken = tokenResult.Value.AccessToken;
        newRefreshToken = tokenResult.Value.NewRefreshToken ?? request.RefreshToken;

        // Update stored refresh token if it was rotated
        if (!string.IsNullOrWhiteSpace(newRefreshToken))
          device.RefreshToken = newRefreshToken;
      }
    }

    // Fallback to client_credentials if refresh token didn't work
    if (string.IsNullOrWhiteSpace(accessToken))
      accessToken = await tokenService.GetTokenForArduinoAsync(device.UserId);

    await _db.SaveChangesAsync();

    return Ok(new
    {
      success = true,
      accesstoken = accessToken,
      refreshtoken = newRefreshToken ?? request.RefreshToken  // Send back for device to store
    });
  }
}
