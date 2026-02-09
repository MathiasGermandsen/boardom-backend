using boardomapi.Database;
using boardomapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.DeviceController;

[ApiController]
[Route("[controller]")]
public class DeviceController : ControllerBase
{
  private readonly AppDbContext _db;

  public DeviceController(AppDbContext db)
  {
    _db = db;
  }

  [HttpPost("addDevice")]
  public async Task<IActionResult> AddDevice([FromBody] AddDeviceRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.DeviceId) || string.IsNullOrWhiteSpace(request.FriendlyName))
    {
      return BadRequest(new { error = "DeviceId and FriendlyName are required" });
    }

    // Check if device already exists (including soft-deleted devices)
    var existing = await _db.Devices
      .IgnoreQueryFilters()
      .FirstOrDefaultAsync(d => d.DeviceId == request.DeviceId);

    if (existing != null)
    {
      // Re-activate if previously soft-deleted
      if (existing.IsDeleted)
      {
        existing.IsDeleted = false;
      }

      existing.FriendlyName = request.FriendlyName;
      await _db.SaveChangesAsync();
      return Ok(new { message = "Device updated", deviceId = existing.DeviceId, friendlyName = existing.FriendlyName });
    }

    // Create new device
    var device = new Device
    {
      DeviceId = request.DeviceId,
      FriendlyName = request.FriendlyName
    };

    _db.Devices.Add(device);
    await _db.SaveChangesAsync();

    return Created($"/device/{device.DeviceId}", new { message = "Device registered", deviceId = device.DeviceId, friendlyName = device.FriendlyName });
  }

  [HttpGet("{deviceId}")]
  public async Task<IActionResult> GetDeviceAsync([FromRoute] string deviceId)
  {
    if (string.IsNullOrWhiteSpace(deviceId))
    {
      return BadRequest(new { error = "DeviceId is required" });
    }

    var device = await _db.Devices
      .AsNoTracking()
      .FirstOrDefaultAsync(d => d.DeviceId == deviceId);

    if (device == null)
    {
      return NotFound(new { error = "Device not found", deviceId });
    }

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
      SensorReadings = sensorData
    });
  }


  [HttpPost("delete/{deviceId}")]
  public async Task<IActionResult> DeleteDeviceAsync([FromRoute] string deviceId)
  {
    if (string.IsNullOrWhiteSpace(deviceId))
    {
      return BadRequest(new { error = "DeviceId is required" });
    }

    var device = await _db.Devices.FindAsync(deviceId);
    if (device == null)
    {
      return NotFound(new { error = "Device not found", deviceId });
    }

    device.IsDeleted = true;
    await _db.SaveChangesAsync();

    return NoContent();
  }
}
