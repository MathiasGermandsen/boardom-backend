using boardomapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.GroupController;

public partial class GroupController
{
  [HttpPost("create")]
  public async Task<IActionResult> CreateGroupAsync([FromBody] CreateGroupRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.GroupName))
      return BadRequest(new { error = "GroupName is required" });

    var existing = await _db.Groups
      .FirstOrDefaultAsync(g => g.GroupName == request.GroupName && !g.IsDeleted && g.UserId == GetUserId());

    if (existing != null)
      return Conflict(new { error = "Group with this name already exists" });

    var group = new Group
    {
      GroupName = request.GroupName,
      UserId = GetUserId()
    };

    _db.Groups.Add(group);
    await _db.SaveChangesAsync();

    return Created($"/group/{group.GroupId}", new
    {
      message = "Group created",
      groupId = group.GroupId,
      groupName = group.GroupName
    });
  }

  [HttpPost("addDevice")]
  public async Task<IActionResult> AddDeviceToGroupAsync([FromBody] AddDeviceToGroupRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.DeviceId))
      return BadRequest(new { error = "DeviceId is required" });

    var (group, error) = await FindGroupOrErrorAsync(request.GroupName);
    if (error != null)
      return error;

    var device = await _db.Devices
    .FirstOrDefaultAsync(d => d.DeviceId == request.DeviceId && d.UserId == GetUserId());

    if (device == null)
      return NotFound(new { error = "Device not found", deviceId = request.DeviceId });

    var alreadyInGroup = await _db.DeviceGroups
      .AnyAsync(dg => dg.GroupId == group!.GroupId && dg.DeviceId == request.DeviceId);

    if (alreadyInGroup)
      return Conflict(new { error = "Device is already in this group" });

    var deviceGroup = new DeviceGroup
    {
      GroupId = group!.GroupId,
      DeviceId = request.DeviceId
    };

    _db.DeviceGroups.Add(deviceGroup);
    await _db.SaveChangesAsync();

    return Created($"/group/{group.GroupId}/device/{request.DeviceId}", new
    {
      message = "Device added to group",
      groupName = group.GroupName,
      deviceId = request.DeviceId
    });
  }
}
