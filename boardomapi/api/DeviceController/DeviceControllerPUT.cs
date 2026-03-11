using boardomapi.Models;
using Microsoft.AspNetCore.Mvc;

namespace boardomapi.Api.DeviceController;

public partial class DeviceController
{
  [HttpPut("edit")]
  public async Task<IActionResult> EditDeviceAsync([FromBody] EditDeviceRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.NewFriendlyName))
      return BadRequest(new { error = "NewFriendlyName is required" });

    var (device, error) = await FindDeviceOrErrorAsync(request.DeviceId);
    if (error != null)
      return error;

    device!.FriendlyName = request.NewFriendlyName;
    await _db.SaveChangesAsync();

    return Ok(new { message = "Device updated", deviceId = device.DeviceId, friendlyName = device.FriendlyName });
  }
}
