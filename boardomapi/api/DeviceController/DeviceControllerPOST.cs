using boardomapi.Models;
using boardomapi.Services;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.DeviceController;

public partial class DeviceController
{
  [HttpPost("add")]
  public async Task<IActionResult> AddDeviceAsync(
    [FromBody] AddDeviceRequest request,
    [FromServices] Auth0TokenService tokenService)
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

    var accessToken = await tokenService.GetTokenForArduinoAsync(GetUserId());
    if (string.IsNullOrWhiteSpace(accessToken))
      return BadRequest(new { error = "Failed to obtain access token" });

    var device = new Device
    {
      DeviceId = request.DeviceId,
      FriendlyName = request.FriendlyName,
      UserId = GetUserId()
    };

    _db.Devices.Add(device);
    await _db.SaveChangesAsync();

    return Created($"/device/{device.DeviceId}", new
    {
      userId = device.UserId,
      accessToken = accessToken,
      message = "Device registered",
      deviceId = device.DeviceId,
      friendlyName = device.FriendlyName,
    });
  }

  [AllowAnonymous]
  [HttpPost("heartbeat")]
  public async Task<IActionResult> HeartbeatAsync(
    [FromBody] DeviceHeartbeatRequest request,
    [FromServices] Auth0TokenService tokenService)
  {
    var (device, error) = await FindDeviceByIdOnlyOrErrorAsync(request.DeviceId);
    if (error != null)
      return error;

    device!.LastHeartbeat = DateTime.UtcNow;

    // Always get a fresh token via client_credentials
    var accessToken = await tokenService.GetTokenForArduinoAsync(device.UserId);
    if (string.IsNullOrWhiteSpace(accessToken))
      return BadRequest(new { error = "Failed to obtain access token" });

    await _db.SaveChangesAsync();

    return Ok(new
    {
      userId = device.UserId,
      success = true,
      accesstoken = accessToken
    });
  }
}
