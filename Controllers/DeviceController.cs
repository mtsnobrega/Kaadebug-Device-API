using kaadebug_device_api.Dtos.Device;
using kaadebug_device_api.Dtos.Readings;
using kaadebug_device_api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace kaadebug_device_api.Controllers;

[ApiController]
[Route("api/device")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IDeviceConfigurationService _configurationService;
    private readonly IPlantMonitoringService _monitoringService;

    public DeviceController(
        IDeviceService deviceService,
        IDeviceConfigurationService configurationService,
        IPlantMonitoringService monitoringService)
    {
        _deviceService = deviceService;
        _configurationService = configurationService;
        _monitoringService = monitoringService;
    }

    /// <summary>Primeiro contato do ESP32. OFFLINE -> ONLINE.</summary>
    [HttpPost("connect")]
    [ProducesResponseType(typeof(DeviceConnectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Connect([FromBody] DeviceConnectRequest request)
    {
        var result = await _deviceService.ConnectAsync(request.DeviceCode);
        return Ok(result);
    }

    /// <summary>ESP32 descobre planta/espécie/parâmetros associados a ele.</summary>
    [HttpGet("{deviceCode}/configuration")]
    [ProducesResponseType(typeof(DeviceConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DeviceNotAssociatedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConfiguration(string deviceCode)
    {
        var result = await _configurationService.GetConfigurationAsync(deviceCode);

        return result.IsAssociated
            ? Ok(result.Configuration)
            : Ok(result.NotAssociated);
    }

    /// <summary>Heartbeat periódico - "estou vivo".</summary>
    [HttpPost("{deviceCode}/heartbeat")]
    [ProducesResponseType(typeof(HeartbeatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Heartbeat(string deviceCode, [FromBody] HeartbeatRequest request)
    {
        var result = await _deviceService.HeartbeatAsync(deviceCode);
        return Ok(result);
    }

    /// <summary>Recebe leituras de sensores, processa e retorna o novo health_status.</summary>
    [HttpPost("{deviceCode}/readings")]
    [ProducesResponseType(typeof(SensorReadingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Readings(string deviceCode, [FromBody] SensorReadingsRequest request)
    {
        var result = await _monitoringService.ProcessReadingsAsync(deviceCode, request);
        return Ok(result);
    }

    /// <summary>Desconexão explícita e controlada. ONLINE -> OFFLINE.</summary>
    [HttpPost("{deviceCode}/disconnect")]
    [ProducesResponseType(typeof(DisconnectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disconnect(string deviceCode)
    {
        var result = await _deviceService.DisconnectAsync(deviceCode);
        return Ok(result);
    }
}
