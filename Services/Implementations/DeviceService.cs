using kaadebug_device_api.Data;
using kaadebug_device_api.Dtos.Device;
using kaadebug_device_api.Exceptions;
using kaadebug_device_api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ConnectionStatusEnum = kaadebug_device_api.Entities.Enums.ConnectionStatus;

namespace kaadebug_device_api.Services.Implementations;
public class DeviceService : IDeviceService
{
    private readonly KaaDebugDbContext _db;

    public DeviceService(KaaDebugDbContext db)
    {
        _db = db;
    }

    public async Task<DeviceConnectResponse> ConnectAsync(string deviceCode)
    {
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.Code == deviceCode)
        ?? throw new DeviceNotFoundException(deviceCode);

        // REGRA DE SEGURANÇA: Se já tem dono (App já associou), bloqueia a chamada!
        // Isso impede que alguém chute o código e tente resetar o status do dispositivo.
        if (device.UserId != null || device.PlantId != null || device.ConnectionStatus == ConnectionStatusEnum.Online)
        {
            throw new InvalidOperationException("Dispositivo já está associado a um usuário. Ação negada.");
        }

        // Se chegou aqui, a ESP não tem dono. 
        // Muda de Offline para Online (Modo de Pareamento) para o App enxergá-la.
        device.ConnectionStatus = ConnectionStatusEnum.Online;
        device.LastHeartbeatAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new DeviceConnectResponse
        {
            DeviceId = device.Id,
            DeviceCode = device.Code,
            ConnectionStatus = device.ConnectionStatus.ToString(),
            Associated = false // Sempre será falso aqui devido à validação acima
        };

        /*
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.Code == deviceCode)
            ?? throw new DeviceNotFoundException(deviceCode);

        device.ConnectionStatus = ConnectionStatusEnum.Online;
        device.LastHeartbeatAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new DeviceConnectResponse
        {
            DeviceId = device.Id,
            DeviceCode = device.Code,
            ConnectionStatus = device.ConnectionStatus.ToString(),
            Associated = device.PlantId != null
        };
        */
    }

    public async Task<HeartbeatResponse> HeartbeatAsync(string deviceCode)
    {
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.Code == deviceCode)
            ?? throw new DeviceNotFoundException(deviceCode);

        var now = DateTime.UtcNow;
        device.LastHeartbeatAt = now;
        //device.ConnectionStatus = ConnectionStatusEnum.Online;

        await _db.SaveChangesAsync();

        return new HeartbeatResponse
        {
            DeviceCode = device.Code,
            //ConnectionStatus = device.ConnectionStatus.ToString(),
            ServerTime = now
        };
    }

    public async Task<DisconnectResponse> DisconnectAsync(string deviceCode)
    {
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.Code == deviceCode)
            ?? throw new DeviceNotFoundException(deviceCode);

        var now = DateTime.UtcNow;
        device.ConnectionStatus = ConnectionStatusEnum.Offline;

        await _db.SaveChangesAsync();

        return new DisconnectResponse
        {
            DeviceCode = device.Code,
            ConnectionStatus = device.ConnectionStatus.ToString(),
            ServerTime = now
        };
    }
}