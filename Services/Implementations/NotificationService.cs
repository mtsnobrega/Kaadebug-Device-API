using kaadebug_device_api.Data;
using kaadebug_device_api.Entities;
using kaadebug_device_api.Services.Interfaces;
using HealthStatusEnum = kaadebug_device_api.Entities.Enums.HealthStatus;
using NotificationPriorityEnum = kaadebug_device_api.Entities.Enums.NotificationPriority;
using SensorTypeEnum = kaadebug_device_api.Entities.Enums.SensorType;

namespace kaadebug_device_api.Services.Implementations;
public class NotificationService : INotificationService
{
    private readonly KaaDebugDbContext _db;

    public NotificationService(KaaDebugDbContext db)
    {
        _db = db;
    }

    public async Task EvaluateAndCreateAsync(
        Plant plant,
        Species species,
        IEnumerable<SensorReading> readings,
        HealthStatusEnum previousStatus,
        HealthStatusEnum newStatus)
    {
        // Regra inicial: só notifica quando o novo status é WARNING ou CRITICAL.
        // Evita spam de notificação a cada lote enquanto a planta permanece no
        // mesmo estado ruim - só notifica de novo se piorou (WARNING -> CRITICAL)
        // ou se é a primeira vez que sai de HEALTHY.
        if (newStatus == HealthStatusEnum.HEALTHY)
        {
            return;
        }

        var worsenedOrNew = newStatus != previousStatus;
        if (!worsenedOrNew)
        {
            return;
        }

        var outOfRange = readings.Where(r => !r.IsWithinIdealRange).ToList();
        if (outOfRange.Count == 0)
        {
            return;
        }

        var priority = newStatus == HealthStatusEnum.CRITICAL
            ? NotificationPriorityEnum.HIGH
            : NotificationPriorityEnum.MEDIUM;

        var message = BuildMessage(outOfRange, species);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = plant.UserId,
            PlantId = plant.Id,
            Message = message,
            Priority = priority,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }

    private static string BuildMessage(List<SensorReading> outOfRange, Species species)
    {
        // Usa a primeira leitura fora da faixa como referência da mensagem.
        var reading = outOfRange.First();

        var (label, min, max) = reading.SensorType switch
        {
            SensorTypeEnum.SOIL_MOISTURE => ("umidade do solo", species.SoilMoistureMin, species.SoilMoistureMax),
            SensorTypeEnum.AIR_HUMIDITY => ("umidade do ar", species.AirHumidityMin, species.AirHumidityMax),
            SensorTypeEnum.TEMPERATURE => ("temperatura", species.TemperatureMin, species.TemperatureMax),
            _ => ("uma medição", 0m, 0m)
        };

        var direction = reading.Value < min ? "abaixo" : "acima";

        return $"A {label} está {direction} do nível ideal ({reading.Value:0.##}, faixa ideal {min:0.##}-{max:0.##}).";
    }
}