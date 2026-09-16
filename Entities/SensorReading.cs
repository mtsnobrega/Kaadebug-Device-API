using kaadebug_device_api.Entities.Enums;

namespace kaadebug_device_api.Entities;
public class SensorReading
{
    public long Id { get; set; }

    public Guid PlantId { get; set; }

    public Guid DeviceId { get; set; }

    public SensorType SensorType { get; set; }

    public decimal Value { get; set; }

    /// <summary>
    /// Calculado pela API (ISensorReadingService) comparando Value com os
    /// limites da Species correspondente. Nunca recebido do ESP32.
    /// </summary>
    public bool IsWithinIdealRange { get; set; }

    public DateTime ReadAt { get; set; }
}
