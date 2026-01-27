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

    // Check if device already exists
    var existing = await _db.Devices.FindAsync(request.DeviceId);
    if (existing != null)
    {
      // Update friendly name if device exists
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


  [HttpDelete("{deviceId}")]
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

    // Use transaction to ensure atomicity of deletion operations
    await using var transaction = await _db.Database.BeginTransactionAsync();
    try
    {
      // Delete associated sensor readings first (directly in database)
      await _db.SensorReadings
        .Where(s => s.DeviceId == deviceId)
        .ExecuteDeleteAsync();

      // Delete the device
      _db.Devices.Remove(device);
      await _db.SaveChangesAsync();
      
      await transaction.CommitAsync();

      return NoContent();
    }
    catch
    {
      await transaction.RollbackAsync();
      throw;
    }
  }
}
