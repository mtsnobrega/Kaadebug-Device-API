namespace kaadebug_device_api.Services.Interfaces;
/// <summary>
/// Resultado do GetConfigurationAsync: ou o device está associado (Configuration preenchido)
/// ou não está (NotAssociated preenchido). O controller decide o formato de resposta a partir disso.
/// </summary>
public class DeviceConfigurationResult
{
    public bool IsAssociated { get; init; }
    public Dtos.Device.DeviceConfigurationResponse? Configuration { get; init; }
    public Dtos.Device.DeviceNotAssociatedResponse? NotAssociated { get; init; }
}

public interface IDeviceConfigurationService
{
    /// <summary>
    /// Localiza Device -> Plant -> Species e monta os parâmetros de monitoramento.
    /// Lança DeviceNotFoundException se o deviceCode não existir.
    /// </summary>
    Task<DeviceConfigurationResult> GetConfigurationAsync(string deviceCode);
}
