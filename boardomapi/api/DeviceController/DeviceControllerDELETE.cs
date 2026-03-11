using Microsoft.AspNetCore.Mvc;

namespace boardomapi.Api.DeviceController;

public partial class DeviceController
{
  [HttpDelete("{deviceId}")]
  public async Task<IActionResult> DeleteDeviceAsync([FromRoute] string deviceId)
  {
    var (device, error) = await FindDeviceOrErrorAsync(deviceId);
    if (error != null)
      return error;

    device!.IsDeleted = true;
    await _db.SaveChangesAsync();

    return NoContent();
  }
}
