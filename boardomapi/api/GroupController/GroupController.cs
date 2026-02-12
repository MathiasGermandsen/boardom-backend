using boardomapi.Database;
using boardomapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.GroupController;

[ApiController]
[Route("[controller]")]
public class GroupController : ControllerBase
{
  private readonly AppDbContext _db;

  public GroupController(AppDbContext db)
  {
    _db = db;
  }

  [HttpPost("create")]
  public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.GroupName))
    {
      return BadRequest(new { error = "GroupName is required" });
    }

    // Check if group already exists
    var existing = await _db.Groups
        .FirstOrDefaultAsync(g => g.GroupName == request.GroupName && !g.IsDeleted);

    if (existing != null)
    {
      return Conflict(new { error = "Group with this name already exists" });
    }

    var group = new Group
    {
      GroupName = request.GroupName
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

  [HttpPut("edit")]
  public async Task<IActionResult> EditGroup([FromBody] EditGroupRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.GroupName) || string.IsNullOrWhiteSpace(request.NewName))
    {
      return BadRequest(new { error = "GroupName and NewName are required" });
    }

    var group = await _db.Groups
        .FirstOrDefaultAsync(g => g.GroupName == request.GroupName);

    if (group == null)
    {
      return NotFound(new { error = "Group not found" });
    }

    if (group.IsDeleted)
    {
      return BadRequest(new { error = "Cannot edit a deleted group" });
    }

    // Check if new name is already taken
    var nameExists = await _db.Groups
        .AnyAsync(g => g.GroupName == request.NewName && g.GroupId != group.GroupId && !g.IsDeleted);

    if (nameExists)
    {
      return Conflict(new { error = "A group with this name already exists" });
    }

    group.GroupName = request.NewName;
    await _db.SaveChangesAsync();

    return Ok(new
    {
      message = "Group updated",
      groupId = group.GroupId,
      groupName = group.GroupName
    });
  }

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

  [HttpPost("addDevice")]
  public async Task<IActionResult> AddDeviceToGroup([FromBody] AddDeviceToGroupRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.GroupName) || string.IsNullOrWhiteSpace(request.DeviceId))
    {
      return BadRequest(new { error = "GroupName and DeviceId are required" });
    }

    var group = await _db.Groups
        .FirstOrDefaultAsync(g => g.GroupName == request.GroupName && !g.IsDeleted);

    if (group == null)
    {
      return NotFound(new { error = "Group not found" });
    }

    var device = await _db.Devices.FindAsync(request.DeviceId);
    if (device == null)
    {
      return NotFound(new { error = "Device not found", deviceId = request.DeviceId });
    }

    // Check if device is already in the group
    var alreadyInGroup = await _db.DeviceGroups
        .AnyAsync(dg => dg.GroupId == group.GroupId && dg.DeviceId == request.DeviceId);

    if (alreadyInGroup)
    {
      return Conflict(new { error = "Device is already in this group" });
    }

    var deviceGroup = new DeviceGroup
    {
      GroupId = group.GroupId,
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

  [HttpDelete("delete/{groupName}")]
  public async Task<IActionResult> DeleteGroup([FromRoute] string groupName)
  {
    if (string.IsNullOrWhiteSpace(groupName))
    {
      return BadRequest(new { error = "Group name is required" });
    }
    
    Group group = await _db.Groups.FirstOrDefaultAsync(g => g.GroupName == groupName);

    if (group == null)
    {
      return NotFound(new { error = "Group not found" });
    }

    _db.Groups.Remove(group);
    await  _db.SaveChangesAsync();
    
    return Ok(new {message = "Group deleted", groupId = group.GroupId, groupName = group.GroupName });
  }
}
