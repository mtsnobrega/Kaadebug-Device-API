namespace kaadebug_device_api.Entities.Enums;
/// <summary>
/// Estado de saúde de uma planta, calculado pelo IPlantHealthService
/// a partir das leituras de sensores recebidas em cada lote.
/// </summary>
public enum HealthStatus
{
    HEALTHY,
    WARNING,
    CRITICAL
}
