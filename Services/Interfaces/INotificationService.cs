using kaadebug_device_api.Entities;
using HealthStatusEnum = kaadebug_device_api.Entities.Enums.HealthStatus;

namespace kaadebug_device_api.Services.Interfaces;
/// <summary>
/// Decide se, a partir do novo health_status e das leituras fora da faixa,
/// deve ser criada uma Notification, e monta userId/plantId/message/priority.
/// </summary>
public interface INotificationService
{
    Task EvaluateAndCreateAsync(
        Plant plant,
        Species species,
        IEnumerable<SensorReading> readings,
        HealthStatusEnum previousStatus,
        HealthStatusEnum newStatus);
}
