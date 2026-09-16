namespace kaadebug_device_api.Dtos.Device;

public class DisconnectResponse
{
    public string DeviceCode { get; set; } = string.Empty;
    public string ConnectionStatus { get; set; } = string.Empty;
    public DateTimeOffset ServerTime { get; set; }
}
