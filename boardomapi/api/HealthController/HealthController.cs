using Microsoft.AspNetCore.Mvc;

namespace boardomapi.Api.HealthController;

/// <summary>
/// Health check endpoint for container orchestration and load balancers.
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
  /// <summary>
  /// Returns the health status of the API.
  /// </summary>
  [HttpGet]
  public IActionResult GetHealth()
  {
    return Ok(new { status = "healthy" });
  }
}
