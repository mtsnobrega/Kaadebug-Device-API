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

        // Regra inicial: se a planta está saudável, não precisa notificar
        if (newStatus == HealthStatusEnum.Healthy)
        {
            return;
        }

        var outOfRange = readings.Where(r => !r.IsWithinIdealRange).ToList();
        if (outOfRange.Count == 0)
        {
            return;
        }

        // Verifica se o status mudou ou se a planta continua no mesmo estado de alerta
        var isRepeatedStatus = newStatus == previousStatus;

        // Lista para guardar todas as notificações que vamos inserir no banco
        var notificationsToSave = new List<Notification>();

        foreach (var reading in outOfRange)
        {
            // 1. Descobre os limites e o nome do sensor
            var (label, min, max) = reading.SensorType switch
            {
                SensorTypeEnum.SoilMoisture => ("Umidade do solo", species.SoilMoistureMin, species.SoilMoistureMax),
                SensorTypeEnum.AirHumidity => ("Umidade do ar", species.AirHumidityMin, species.AirHumidityMax),
                SensorTypeEnum.Temperature => ("Temperatura", species.TemperatureMin, species.TemperatureMax),
                _ => ("Uma medição", 0m, 0m)
            };

            // 2. Calcula a gravidade individual desta leitura específica
            var severity = GetReadingSeverity(reading.Value, min, max);
            var severityTag = severity == HealthStatusEnum.Critical ? "CRÍTICO" : "AVISO";

            // A prioridade no banco agora reflete a gravidade do sensor individual
            var priority = severity == HealthStatusEnum.Critical
                ? NotificationPriorityEnum.High
                : NotificationPriorityEnum.Medium;

            var direction = reading.Value < min ? "abaixo" : "acima";

            // 3. Monta a mensagem específica deste sensor
            var message = $"{severityTag}: {label} está {direction} ({reading.Value:0.##}, ideal {min:0.##}-{max:0.##})";
            if (isRepeatedStatus)
            {
                message = $"Alerta Contínuo: {message}";
            }

            // 4. Cria a notificação individual e adiciona na lista
            notificationsToSave.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = plant.UserId,
                PlantId = plant.Id,
                Message = message,
                Priority = priority,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        // 5. Salva todas as linhas de uma vez no banco de dados
        _db.Notifications.AddRange(notificationsToSave);
        await _db.SaveChangesAsync();
    }




        /*
        // Regra inicial: se a planta está saudável, não precisa notificar
        if (newStatus == HealthStatusEnum.Healthy)
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

        var priority = newStatus == HealthStatusEnum.Critical
            ? NotificationPriorityEnum.High
            : NotificationPriorityEnum.Medium;

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
        */
        /*
    private static string BuildMessage(List<SensorReading> outOfRange, Species species, bool isRepeatedStatus)
    {
        var alerts = outOfRange.Select(reading =>
        {
            // 1. Descobre os limites para o tipo de sensor atual
            var (label, min, max) = reading.SensorType switch
            {
                SensorTypeEnum.SoilMoisture => ("Umidade do solo", species.SoilMoistureMin, species.SoilMoistureMax),
                SensorTypeEnum.AirHumidity => ("Umidade do ar", species.AirHumidityMin, species.AirHumidityMax),
                SensorTypeEnum.Temperature => ("Temperatura", species.TemperatureMin, species.TemperatureMax),
                _ => ("Uma medição", 0m, 0m)
            };

            var direction = reading.Value < min ? "abaixo" : "acima";

            // 2. Calcula a gravidade individual com base na sua margem de 20%
            var severity = GetReadingSeverity(reading.Value, min, max);
            var severityTag = severity == HealthStatusEnum.Critical ? "🔴 CRÍTICO" : "🟡 AVISO";

            // 3. Monta a linha específica deste sensor
            return $"{severityTag}: {label} está {direction} ({reading.Value:0.##}, ideal {min:0.##}-{max:0.##})";
        });

        // Junta cada alerta em uma nova linha
        var baseMessage = string.Join("\n", alerts);

        if (isRepeatedStatus)
        {
            return $"Alerta Contínuo:\n{baseMessage}";
        }

        return baseMessage;
    }*/

    /// <summary>
    /// Calcula se o desvio isolado da leitura ultrapassa os 20% (0.20m)
    /// </summary>
    /// <summary>
    /// Calcula se o desvio isolado da leitura ultrapassa os 20% (0.20m)
    /// </summary>
    private static HealthStatusEnum GetReadingSeverity(decimal readingValue, decimal min, decimal max)
    {
        var nearestLimit = readingValue < min ? min : max;

        if (nearestLimit == 0)
        {
            return HealthStatusEnum.Critical;
        }

        var deviation = Math.Abs(readingValue - nearestLimit) / nearestLimit;
        const decimal ToleranceMargin = 0.20m;

        return deviation > ToleranceMargin ? HealthStatusEnum.Critical : HealthStatusEnum.Warning;
    }



    /*
     * versão 2 
     * 
    // Cria um texto de alerta para cada sensor que estiver fora da faixa
    var alerts = outOfRange.Select(reading =>
    {
        var (label, min, max) = reading.SensorType switch
        {
            SensorTypeEnum.SoilMoisture => ("A umidade do solo", species.SoilMoistureMin, species.SoilMoistureMax),
            SensorTypeEnum.AirHumidity => ("A umidade do ar", species.AirHumidityMin, species.AirHumidityMax),
            SensorTypeEnum.Temperature => ("A temperatura", species.TemperatureMin, species.TemperatureMax),
            _ => ("Uma medição", 0m, 0m)
        };

        var direction = reading.Value < min ? "abaixo" : "acima";

        return $"{label.ToLower()} está {direction} ({reading.Value:0.##}, ideal: {min:0.##}-{max:0.##})";
    });

    // Junta todos os alertas separados por vírgula e adiciona um ponto final
    var baseMessage = string.Join(", e ", alerts) + ".";

    // Deixa a primeira letra maiúscula
    baseMessage = char.ToUpper(baseMessage[0]) + baseMessage.Substring(1);

    if (isRepeatedStatus)
    {
        return $"Alerta Contínuo: {baseMessage}";
    }

    return baseMessage;

    */


    /*
    // Usa a primeira leitura fora da faixa como referência da mensagem.
    var reading = outOfRange.First();

    var (label, min, max) = reading.SensorType switch
    {
        SensorTypeEnum.SoilMoisture => ("umidade do solo", species.SoilMoistureMin, species.SoilMoistureMax),
        SensorTypeEnum.AirHumidity => ("umidade do ar", species.AirHumidityMin, species.AirHumidityMax),
        SensorTypeEnum.Temperature => ("temperatura", species.TemperatureMin, species.TemperatureMax),
        _ => ("uma medição", 0m, 0m)
    };

    var direction = reading.Value < min ? "abaixo" : "acima";

    return $"A {label} está {direction} do nível ideal ({reading.Value:0.##}, faixa ideal {min:0.##}-{max:0.##}).";

    */
}