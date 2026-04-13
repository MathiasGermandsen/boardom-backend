using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.GroupController;

public partial class GroupController
{
  [HttpGet("getAll")]
  public async Task<ActionResult> GetAllGroupsAsync()
  {
    var groups = await _db.Groups
      .AsNoTracking()
      .Where(g => g.UserId == GetUserId())
      .Select(g => new
      {
        g.GroupId,
        g.GroupName,
        g.DateAdded,
        Devices = _db.DeviceGroups
          .Where(dg => dg.GroupId == g.GroupId)
          .Select(dg => new
          {
            dg.Device!.DeviceId,
            dg.Device.FriendlyName,
            DeviceDateAdded = dg.Device.DateAdded,
            AddedToGroupAt = dg.DateAdded
          })
          .ToList()
      })
      .ToListAsync();

    return Ok(groups);
  }
}
