using kaadebug_device_api.Dtos.Device;

namespace kaadebug_device_api.Services.Interfaces;
/// <summary>
/// Responsável pelo ciclo de vida de conexão do device: connect, heartbeat, disconnect.
/// Não lida com planta/espécie/leituras - isso é responsabilidade de outros services.
/// </summary>
public interface IDeviceService
{
    Task<DeviceConnectResponse> ConnectAsync(string deviceCode);

    Task<HeartbeatResponse> HeartbeatAsync(string deviceCode);

    Task<DisconnectResponse> DisconnectAsync(string deviceCode);
}
