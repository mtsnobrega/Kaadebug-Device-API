using kaadebug_device_api.Dtos.Readings;

namespace kaadebug_device_api.Services.Interfaces;
/// <summary>
/// Orquestra o fluxo completo do endpoint de leituras: localizar device/plant/species,
/// delegar validação+persistência ao ISensorReadingService, recalcular health_status
/// via IPlantHealthService e avaliar notificações via INotificationService.
/// </summary>
public interface IPlantMonitoringService
{
    Task<SensorReadingsResponse> ProcessReadingsAsync(string deviceCode, SensorReadingsRequest request);
}