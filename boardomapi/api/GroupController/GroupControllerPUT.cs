using boardomapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.GroupController;

public partial class GroupController
{
  [HttpPut("edit")]
  public async Task<IActionResult> EditGroupAsync([FromBody] EditGroupRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.NewName))
      return BadRequest(new { error = "NewName is required" });

    var (group, error) = await FindGroupOrErrorAsync(request.GroupName);
    if (error != null)
      return error;

    var nameExists = await _db.Groups
      .AnyAsync(g => g.GroupName == request.NewName && g.GroupId != group!.GroupId && !g.IsDeleted);

    if (nameExists)
      return Conflict(new { error = "A group with this name already exists" });

    group!.GroupName = request.NewName;
    await _db.SaveChangesAsync();

    return Ok(new
    {
      message = "Group updated",
      groupId = group.GroupId,
      groupName = group.GroupName
    });
  }
}
