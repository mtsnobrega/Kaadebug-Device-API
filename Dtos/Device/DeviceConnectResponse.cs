namespace kaadebug_device_api.Dtos.Device;
    public class DeviceConnectResponse
{
    public Guid DeviceId { get; set; }
    public string DeviceCode { get; set; } = string.Empty;
    public string ConnectionStatus { get; set; } = string.Empty;
    public bool Associated { get; set; }
}
