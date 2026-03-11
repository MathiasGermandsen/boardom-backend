using boardomapi.Database;
using boardomapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.DeviceController;

[ApiController]
[Route("[controller]")]
public partial class DeviceController : ControllerBase
{
  private readonly AppDbContext _db;

  public DeviceController(AppDbContext db)
  {
    _db = db;
  }

  private async Task<(Device? device, IActionResult? error)> FindDeviceOrErrorAsync(string deviceId)
  {
    if (string.IsNullOrWhiteSpace(deviceId))
      return (null, BadRequest(new { error = "DeviceId is required" }));

    var device = await _db.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
    if (device == null)
      return (null, NotFound(new { error = "Device not found", deviceId }));

    return (device, null);
  }
}
