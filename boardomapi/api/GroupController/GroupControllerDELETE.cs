using Microsoft.AspNetCore.Mvc;

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
}
