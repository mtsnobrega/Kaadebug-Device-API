namespace kaadebug_device_api.Entities;
public class Species
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    public decimal SoilMoistureMin { get; set; }
    public decimal SoilMoistureMax { get; set; }

    public decimal AirHumidityMin { get; set; }
    public decimal AirHumidityMax { get; set; }

    public decimal TemperatureMin { get; set; }
    public decimal TemperatureMax { get; set; }

    public string? CareInfo { get; set; }

    public int IrrigationIntervalHours { get; set; }
    public int ReadingFrequency { get; set; }
}