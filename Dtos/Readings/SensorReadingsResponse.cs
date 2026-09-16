namespace kaadebug_device_api.Dtos.Readings;

public class SensorReadingsResponse
{
    public string DeviceCode { get; set; } = string.Empty;
    public Guid PlantId { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public int ProcessedReadings { get; set; }
    public string PlantHealthStatus { get; set; } = string.Empty;
}

