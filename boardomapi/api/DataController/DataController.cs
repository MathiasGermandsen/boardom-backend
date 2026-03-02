using boardomapi.Database;
using boardomapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.DataController;

[ApiController]
[Route("[controller]")]
public class DataController : ControllerBase
{
  private readonly AppDbContext _db;

  public DataController(AppDbContext db)
  {
    _db = db;
  }

  [HttpPost("sensorData")]
  public async Task<IActionResult> PostSensorData([FromBody] SensorDataRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.DeviceId))
    {
      return BadRequest(new { error = "DeviceId is required" });
    }

    var deviceExists = await _db.Devices.AnyAsync(d => d.DeviceId == request.DeviceId);
    if (!deviceExists)
    {
      return NotFound(new { error = "Device not found", deviceId = request.DeviceId });
    }

    var sensorData = new SensorData
    {
      DeviceId = request.DeviceId,
      Temperature = request.Temperature,
      Humidity = request.Humidity,
      Pressure = request.Pressure,
      Light = request.Light,
      Moisture = request.Moisture
    };

    _db.SensorReadings.Add(sensorData);
    await _db.SaveChangesAsync();

    return Created($"/data/{sensorData.PKey}", new
    {
      message = "Sensor data recorded",
      id = sensorData.PKey,
      deviceId = sensorData.DeviceId,
      dateAdded = sensorData.DateAdded
    });
  }


  [HttpGet("sensorData/{deviceId}")]
  public async Task<ActionResult<PaginatedResponse<SensorDataResponse>>> GetSensorDataAsync(
      [FromRoute] string deviceId,
      [FromQuery] int page = 1,
      [FromQuery] DateTime? startDate = null,
      [FromQuery] DateTime? endDate = null)
  {
    if (string.IsNullOrWhiteSpace(deviceId))
      return BadRequest(new { error = "DeviceId is required" });

    if (page < 1)
      return BadRequest(new { error = "Page must be greater than or equal to 1" });

    var deviceExists = await _db.Devices.AnyAsync(d => d.DeviceId == deviceId);
    if (!deviceExists)
      return NotFound(new { error = "Device not found", deviceId });

    const int pageSize = 100;

    var query = _db.SensorReadings
        .AsNoTracking()
        .Where(s => s.DeviceId == deviceId);

    if (startDate.HasValue)
      query = query.Where(s => s.DateAdded >= startDate.Value.ToUniversalTime());

    if (endDate.HasValue)
      query = query.Where(s => s.DateAdded <= endDate.Value.ToUniversalTime());

    query = query.OrderByDescending(s => s.DateAdded);

    var totalCount = await query.CountAsync();
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(s => new SensorDataResponse(
            s.PKey,
            s.DeviceId,
            s.DateAdded,
            s.Temperature,
            s.Humidity,
            s.Pressure,
            s.Light,
            s.Moisture))
        .ToListAsync();

    var response = new PaginatedResponse<SensorDataResponse>(
        items, page, pageSize, totalCount, totalPages);

    return Ok(response);
  }
}
