using kaadebug_device_api.Data;
using kaadebug_device_api.Dtos.Readings;
using kaadebug_device_api.Entities;
using kaadebug_device_api.Entities.Enums;
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
            SensorTypeEnum.SoilMoisture => value >= species.SoilMoistureMin && value <= species.SoilMoistureMax,
            SensorTypeEnum.AirHumidity => value >= species.AirHumidityMin && value <= species.AirHumidityMax,
            SensorTypeEnum.Temperature => value >= species.TemperatureMin && value <= species.TemperatureMax,
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
            // 1. Criamos uma variável para guardar o enum convertido
            SensorType sensorType;

            // 2. Fazemos um mapeamento manual do texto vindo do ESP32 para o Enum do C#
            switch (item.SensorType?.ToUpper().Trim())
            {
                case "SOIL_MOISTURE":
                    sensorType = SensorType.SoilMoisture;
                    break;
                case "AIR_HUMIDITY":
                    sensorType = SensorType.AirHumidity;
                    break;
                case "TEMPERATURE":
                    sensorType = SensorType.Temperature;
                    break;
                default:
                    // 3. Caso o ESP32 envie algo totalmente diferente, tenta o TryParse padrão como plano B
                    if (!Enum.TryParse<SensorType>(item.SensorType, ignoreCase: true, out sensorType))
                    {
                        throw new InvalidSensorReadingException(
                            $"Tipo de sensor '{item.SensorType}' é inválido. Valores aceitos: SOIL_MOISTURE, AIR_HUMIDITY, TEMPERATURE");
                    }
                    break;
            }

            var withinRange = IsWithinRange(sensorType, item.Value, species);

            entities.Add(new SensorReading
            {
                PlantId = plantId,
                DeviceId = deviceId,
                SensorType = sensorType, // Agora a variável mapeada vai aqui corretinha
                Value = item.Value,
                IsWithinIdealRange = withinRange,
                ReadAt = readAt
            });

            /*
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
            });*/
        }

        _db.SensorReadings.AddRange(entities);
        await _db.SaveChangesAsync();

        return entities;
    }
}
