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
      .Select(g => new
      {
        g.GroupId,
        g.GroupName,
        g.CreatedAt,
        Devices = _db.DeviceGroups
          .Where(dg => dg.GroupId == g.GroupId)
          .Select(dg => new
          {
            dg.Device!.DeviceId,
            dg.Device.FriendlyName,
            dg.Device.CreatedAt,
            dg.AddedAt
          })
          .ToList()
      })
      .ToListAsync();

    return Ok(groups);
  }
}
