using boardomapi.Database;
using boardomapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.GroupController;

[ApiController]
[Route("[controller]")]
public partial class GroupController : ControllerBase
{
  private readonly AppDbContext _db;

  public GroupController(AppDbContext db)
  {
    _db = db;
  }
  private async Task<(Group? group, IActionResult? error)> FindGroupOrErrorAsync(string groupName)
  {
    if (string.IsNullOrWhiteSpace(groupName))
      return (null, BadRequest(new { error = "GroupName is required" }));

    var group = await _db.Groups
      .FirstOrDefaultAsync(g => g.GroupName == groupName && !g.IsDeleted);

    if (group == null)
      return (null, NotFound(new { error = "Group not found" }));

    return (group, null);
  }
}
