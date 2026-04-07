using boardomapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.DeviceController;

public partial class DeviceController
{
  [HttpGet("{deviceId}")]
  public async Task<IActionResult> GetDeviceAsync([FromRoute] string deviceId)
  {
    if (string.IsNullOrWhiteSpace(deviceId))
      return BadRequest(new { error = "DeviceId is required" });

    var device = await _db.Devices
      .AsNoTracking()
      .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == GetUserId());

    if (device == null)
      return NotFound(new { error = "Device not found", deviceId });

    var sensorData = await _db.SensorReadings
      .AsNoTracking()
      .Where(s => s.DeviceId == deviceId)
      .OrderByDescending(s => s.DateAdded)
      .ToListAsync();

    return Ok(new
    {
      device.DeviceId,
      device.FriendlyName,
      device.CreatedAt,
      device.LastHeartbeat,
      SensorReadings = sensorData
    });
  }

  [HttpGet("getAll")]
  public async Task<ActionResult> GetAllDevicesAsync()
  {
    var devices = await _db.Devices
      .AsNoTracking()
      .Where(d => d.UserId == GetUserId())
      .Select(d => new
      {
        d.DeviceId,
        d.FriendlyName,
        d.CreatedAt,
        d.LastHeartbeat
      })
      .ToListAsync();

    return Ok(devices);
  }
}
