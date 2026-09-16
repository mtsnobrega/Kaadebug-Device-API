using kaadebug_device_api.Entities;
using HealthStatusEnum = kaadebug_device_api.Entities.Enums.HealthStatus;

namespace kaadebug_device_api.Services.Interfaces;
/// <summary>
/// Determina o HealthStatus da planta a partir do lote de leituras recém-processado.
///
/// Regra (confirmada para esta primeira versão):
/// - HEALTHY: todas as leituras do lote estão dentro da faixa ideal da Species.
/// - WARNING: existe ao menos uma leitura fora da faixa, mas o desvio em relação
///   ao limite mais próximo (min ou max) é de até 20%.
/// - CRITICAL: existe ao menos uma leitura fora da faixa com desvio maior que 20%
///   em relação ao limite mais próximo.
///
/// O desvio é calculado como: |valor - limite_mais_próximo| / limite_mais_próximo.
/// Considera-se apenas o lote atual de leituras, não o histórico acumulado.
/// </summary>
public interface IPlantHealthService
{
    HealthStatusEnum DetermineHealthStatus(Species species, IEnumerable<SensorReading> readings);
}