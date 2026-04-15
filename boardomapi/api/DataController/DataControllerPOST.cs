using boardomapi.Database;
using boardomapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Api.DataController;

public partial class DataController
{
  [AllowAnonymous]
  [HttpPost("sensorData")]
  public async Task<IActionResult> PostSensorData([FromBody] SensorDataRequest request)
  {
    _logger.LogInformation("sensor data POST received for device: {DeviceId}", request.DeviceId);

    if (string.IsNullOrWhiteSpace(request.DeviceId))
    {
      _logger.LogWarning("DeviceId Is missing");
      return BadRequest(new { error = "DeviceId is required" });
    }

    var deviceExists = await _db.Devices.IgnoreQueryFilters().AnyAsync(d => d.DeviceId == request.DeviceId);
    if (!deviceExists)
    {
      _logger.LogWarning("Device not found: {DeviceId}", request.DeviceId);
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

    _logger.LogInformation("Sensor data saved - DeviceId: {DeviceId}, RecordId: {RecordId}", 
    request.DeviceId, sensorData.PKey);

    return Created($"/data/{sensorData.PKey}", new
    {
      message = "Sensor data recorded",
      id = sensorData.PKey,
      deviceId = sensorData.DeviceId,
      dateAdded = sensorData.DateAdded
    });
  }
}
