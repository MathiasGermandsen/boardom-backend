using boardomapi.Database;
using boardomapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.DataController;

[ApiController]
[Route("[controller]")]
public partial class DataController : ControllerBase
{
  private readonly AppDbContext _db;

  public DataController(AppDbContext db)
  {
    _db = db;
  }
}
