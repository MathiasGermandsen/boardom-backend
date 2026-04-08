using boardomapi.Models;
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

  [HttpPost("heartbeat")]
  public async Task<IActionResult> HeartbeatAsync([FromBody] DeviceHeartbeatRequest request)
  {
    var (device, error) = await FindDeviceOrErrorAsync(request.DeviceId);
    if (error != null)
      return error;

    device!.LastHeartbeat = DateTime.UtcNow;
    await _db.SaveChangesAsync();

    return Ok(new { success = true });
  }
}
