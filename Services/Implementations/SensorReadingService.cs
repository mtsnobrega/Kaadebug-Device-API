using kaadebug_device_api.Data;
using kaadebug_device_api.Dtos.Readings;
using kaadebug_device_api.Entities;
using kaadebug_device_api.Exceptions;
using kaadebug_device_api.Services.Interfaces;
using SensorTypeEnum = kaadebug_device_api.Entities.Enums.SensorType;

namespace kaadebug_device_api.Services.Implementations;
public class SensorReadingService : ISensorReadingService
{
    private readonly KaaDebugDbContext _db;

    public SensorReadingService(KaaDebugDbContext db)
    {
        _db = db;
    }

    public bool IsWithinRange(SensorTypeEnum sensorType, decimal value, Species species)
    {
        return sensorType switch
        {
            SensorTypeEnum.SOIL_MOISTURE => value >= species.SoilMoistureMin && value <= species.SoilMoistureMax,
            SensorTypeEnum.AIR_HUMIDITY => value >= species.AirHumidityMin && value <= species.AirHumidityMax,
            SensorTypeEnum.TEMPERATURE => value >= species.TemperatureMin && value <= species.TemperatureMax,
            _ => throw new InvalidSensorReadingException($"Tipo de sensor '{sensorType}' não é suportado.")
        };
    }

    public async Task<List<SensorReading>> SaveReadingsAsync(
        Guid plantId,
        Guid deviceId,
        Species species,
        IEnumerable<SensorReadingItem> readings,
        DateTime readAt)
    {
        var entities = new List<SensorReading>();

        foreach (var item in readings)
        {
            if (!Enum.TryParse<SensorTypeEnum>(item.SensorType, ignoreCase: true, out var sensorType))
            {
                throw new InvalidSensorReadingException(
                    $"Tipo de sensor '{item.SensorType}' é inválido. Valores aceitos: " +
                    string.Join(", ", Enum.GetNames<SensorTypeEnum>()));
            }

            var withinRange = IsWithinRange(sensorType, item.Value, species);

            entities.Add(new SensorReading
            {
                PlantId = plantId,
                DeviceId = deviceId,
                SensorType = sensorType,
                Value = item.Value,
                IsWithinIdealRange = withinRange,
                ReadAt = readAt
            });
        }

        _db.SensorReadings.AddRange(entities);
        await _db.SaveChangesAsync();

        return entities;
    }
}
