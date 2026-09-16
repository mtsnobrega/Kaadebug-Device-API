namespace kaadebug_device_api.Dtos.Device;

public class RangeDto
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
}

public class MonitoringParametersDto
{
    public RangeDto SoilMoisture { get; set; } = new();
    public RangeDto AirHumidity { get; set; } = new();
    public RangeDto Temperature { get; set; } = new();
}

public class PlantSummaryDto
{
    public Guid PlantId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SpeciesSummaryDto
{
    public Guid SpeciesId { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Resposta quando o device tem planta associada: traz tudo que o ESP32
/// precisa guardar em memória para monitorar sem consultar o banco a cada leitura.
/// </summary>
public class DeviceConfigurationResponse
{
    public Guid DeviceId { get; set; }
    public string DeviceCode { get; set; } = string.Empty;
    public bool Associated { get; set; } = true;
    public PlantSummaryDto Plant { get; set; } = new();
    public SpeciesSummaryDto Species { get; set; } = new();
    public MonitoringParametersDto MonitoringParameters { get; set; } = new();
}

/// <summary>
/// Resposta quando o device ainda não tem planta associada (HTTP 200).
/// O ESP32 recebe isso e não tem parâmetros para monitorar ainda -
/// ele deve tentar novamente mais tarde (ex: no próximo connect/heartbeat).
/// </summary>
public class DeviceNotAssociatedResponse
{
    public Guid DeviceId { get; set; }
    public string DeviceCode { get; set; } = string.Empty;
    public bool Associated { get; set; } = false;
    public string Message { get; set; } = "Dispositivo ainda não está associado a nenhuma planta.";
}