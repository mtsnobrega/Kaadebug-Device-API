using NpgsqlTypes;

namespace kaadebug_device_api.Entities.Enums;
/// <summary>
/// Estado de saúde de uma planta, calculado pelo IPlantHealthService
/// a partir das leituras de sensores recebidas em cada lote.
/// </summary>
public enum HealthStatus
{
    [PgName("HEALTHY")]
    Healthy,
    [PgName("WARNING")]
    Warning,
    [PgName("CRITICAL")]
    Critical
}
