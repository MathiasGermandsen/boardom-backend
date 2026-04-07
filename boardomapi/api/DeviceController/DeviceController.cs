using boardomapi.Database;
using boardomapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace boardomapi.Api.DeviceController;

[Authorize]
[ApiController]
[Route("[controller]")]
public partial class DeviceController : ControllerBase
{
  private readonly AppDbContext _db;

  public DeviceController(AppDbContext db)
  {
    _db = db;
  }

  private string GetUserId() =>
    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;

  private async Task<(Device? device, IActionResult? error)> FindDeviceOrErrorAsync(string deviceId)
  {
    if (string.IsNullOrWhiteSpace(deviceId))
      return (null, BadRequest(new { error = "DeviceId is required" }));

    Device? device = await _db.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == GetUserId());

    if (device == null)
      return (null, NotFound(new { error = "Device not found", deviceId }));

      
    if (device.IsDeleted)
    {
      return (null, StatusCode(410, new { error = "Device is deleted", deviceId}));
    }

    return (device, null);
  }
}
