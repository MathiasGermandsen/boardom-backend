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
}
