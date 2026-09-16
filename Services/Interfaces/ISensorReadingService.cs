using kaadebug_device_api.Dtos.Readings;
using kaadebug_device_api.Entities;
using SensorTypeEnum = kaadebug_device_api.Entities.Enums.SensorType;

namespace kaadebug_device_api.Services.Interfaces;
/// <summary>
/// Responsável por validar cada leitura individual contra os limites da Species
/// e persistir as entidades SensorReading. Não decide health_status da planta
/// (isso é do IPlantHealthService) nem cria notificações.
/// </summary>
public interface ISensorReadingService
{
    /// <summary>Compara um valor com os limites min/max da espécie para o tipo de sensor informado.</summary>
    bool IsWithinRange(SensorTypeEnum sensorType, decimal value, Species species);

    /// <summary>
    /// Converte os itens do request em entidades SensorReading (calculando IsWithinIdealRange
    /// para cada uma) e persiste no banco. Retorna as entidades criadas para uso posterior
    /// (cálculo de health_status, notificações).
    /// Lança InvalidSensorReadingException se algum sensorType for inválido.
    /// </summary>
    Task<List<SensorReading>> SaveReadingsAsync(
        Guid plantId,
        Guid deviceId,
        Species species,
        IEnumerable<SensorReadingItem> readings,
        DateTime readAt);
}
