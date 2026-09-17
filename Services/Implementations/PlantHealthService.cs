using kaadebug_device_api.Entities;
using kaadebug_device_api.Services.Interfaces;
using HealthStatusEnum = kaadebug_device_api.Entities.Enums.HealthStatus;
using SensorTypeEnum = kaadebug_device_api.Entities.Enums.SensorType;

namespace kaadebug_device_api.Services.Implementations;
public class PlantHealthService : IPlantHealthService
{
    /// <summary>Margem de tolerância confirmada para diferenciar WARNING de CRITICAL.</summary>
    private const decimal ToleranceMargin = 0.20m;

    public HealthStatusEnum DetermineHealthStatus(Species species, IEnumerable<SensorReading> readings)
    {
        var readingsList = readings.ToList();

        var outOfRange = readingsList.Where(r => !r.IsWithinIdealRange).ToList();

        if (outOfRange.Count == 0)
        {
            return HealthStatusEnum.Healthy;
        }

        // Se qualquer leitura fora da faixa tiver desvio > 20% em relação ao
        // limite mais próximo, a planta é CRITICAL. Caso contrário, WARNING.
        var hasCritical = outOfRange.Any(reading => DeviationRatio(reading, species) > ToleranceMargin);

        return hasCritical ? HealthStatusEnum.Critical : HealthStatusEnum.Warning;
    }

    /// <summary>
    /// Calcula o desvio relativo do valor lido em relação ao limite (min ou max)
    /// mais próximo da faixa ideal da espécie, para o tipo de sensor da leitura.
    /// </summary>
    private static decimal DeviationRatio(SensorReading reading, Species species)
    {
        var (min, max) = reading.SensorType switch
        {
            SensorTypeEnum.SoilMoisture => (species.SoilMoistureMin, species.SoilMoistureMax),
            SensorTypeEnum.AirHumidity => (species.AirHumidityMin, species.AirHumidityMax),
            SensorTypeEnum.Temperature => (species.TemperatureMin, species.TemperatureMax),
            _ => (0m, 0m)
        };

        var nearestLimit = reading.Value < min ? min : max;

        if (nearestLimit == 0)
        {
            // Evita divisão por zero em espécies mal configuradas; trata como desvio máximo.
            return decimal.MaxValue;
        }

        return Math.Abs(reading.Value - nearestLimit) / nearestLimit;
    }
}