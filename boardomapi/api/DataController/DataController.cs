using boardomapi.Database;
using boardomapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.DataController;

[Authorize]
[ApiController]
[Microsoft.AspNetCore.Components.Route("[controller]")]
public partial class DataController : ControllerBase
{
  private readonly AppDbContext _db;
  private readonly ILogger<DataController> _logger;
  public DataController(AppDbContext db, ILogger<DataController> logger)
  {
    _db = db;
    _logger = logger;
  }

  private string GetUserId() =>
    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
}
