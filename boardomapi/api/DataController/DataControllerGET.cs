using boardomapi.Database;
using boardomapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.DataController;

public partial class DataController
{
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
