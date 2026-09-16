namespace kaadebug_device_api.Dtos.Device;
public class HeartbeatRequest
{
    /// <summary>Informativo apenas - o servidor não usa isso como fonte de verdade do horário.</summary>
    public DateTimeOffset? SentAt { get; set; }
}

public class HeartbeatResponse
{
    public string DeviceCode { get; set; } = string.Empty;
    public string ConnectionStatus { get; set; } = string.Empty;
    public DateTimeOffset ServerTime { get; set; }
}