using boardomapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace boardomapi.Api.GroupController;

public partial class GroupController
{
  [HttpDelete("{groupName}")]
  public async Task<IActionResult> DeleteGroupAsync([FromRoute] string groupName)
  {
    var (group, error) = await FindGroupOrErrorAsync(groupName);
    if (error != null)
      return error;

    _db.Groups.Remove(group!);
    await _db.SaveChangesAsync();

    return NoContent();
  }

  [HttpDelete("deleteFrom")]
  public async Task<IActionResult> DeleteFromGroupAsync([FromBody] DeleteDeviceFromGroupRequest request)
  {
    var (group, error) = await FindGroupOrErrorAsync(request.GroupName);
    if (error != null)
      return error;

      Device? dev = await _db.Devices
      .FirstOrDefaultAsync(d => d.DeviceId == request.DeviceId && d.UserId == GetUserId());

    if (dev == null)
    {
      return NotFound(new { error = "Device not found", deviceId = request.DeviceId });
    }

    bool inGroup = await _db.DeviceGroups.AnyAsync(dg => dg.GroupId == group!.GroupId && dg.DeviceId == request.DeviceId);

    if (!inGroup)
    {
      return NotFound(new {error = "Device not in group", deviceId = request.DeviceId, groupName = group.GroupName});
    }

    DeviceGroup? deviceInGroup = await _db.DeviceGroups.FirstOrDefaultAsync(dg => dg.GroupId == group!.GroupId && dg.DeviceId == dev.DeviceId);
    _db.DeviceGroups.Remove(deviceInGroup);
    await _db.SaveChangesAsync();

    return Ok();
  }

  
}
