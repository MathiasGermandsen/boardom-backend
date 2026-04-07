using boardomapi.Database;
using boardomapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.DataController;

[Authorize]
[ApiController]
[Route("[controller]")]
public partial class DataController : ControllerBase
{
  private readonly AppDbContext _db;

  public DataController(AppDbContext db)
  {
    _db = db;
  }

  private string GetUserId() =>
    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
}
