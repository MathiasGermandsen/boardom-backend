namespace boardomapi.Models;

public record SensorDataRequest(
    string DeviceId,
    double Temperature,
    double Humidity,
    double Pressure,
    double Light,
    double Moisture
);

public record CreateGroupRequest(string GroupName);
public record EditGroupRequest(string GroupName, string NewName);
public record AddDeviceToGroupRequest(string GroupName, string DeviceId);


public record AddDeviceRequest(string DeviceId, string FriendlyName);

