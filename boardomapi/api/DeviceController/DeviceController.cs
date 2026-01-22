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
}

