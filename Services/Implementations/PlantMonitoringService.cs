using kaadebug_device_api.Data;
using kaadebug_device_api.Dtos.Readings;
using kaadebug_device_api.Exceptions;
using kaadebug_device_api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ConnectionStatusEnum = kaadebug_device_api.Entities.Enums.ConnectionStatus;

namespace kaadebug_device_api.Services.Implementations;
public class PlantMonitoringService : IPlantMonitoringService
{
    private readonly KaaDebugDbContext _db;
    private readonly ISensorReadingService _sensorReadingService;
    private readonly IPlantHealthService _plantHealthService;
    private readonly INotificationService _notificationService;

    public PlantMonitoringService(
        KaaDebugDbContext db,
        ISensorReadingService sensorReadingService,
        IPlantHealthService plantHealthService,
        INotificationService notificationService)
    {
        _db = db;
        _sensorReadingService = sensorReadingService;
        _plantHealthService = plantHealthService;
        _notificationService = notificationService;
    }

    public async Task<SensorReadingsResponse> ProcessReadingsAsync(string deviceCode, SensorReadingsRequest request)
    {
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.Code == deviceCode)
            ?? throw new DeviceNotFoundException(deviceCode);

        if (device.PlantId is null)
        {
            throw new DeviceNotAssociatedException(deviceCode);
        }

        var plant = await _db.Plants
            .Include(p => p.Species)
            .FirstOrDefaultAsync(p => p.Id == device.PlantId.Value);

        if (plant is null || plant.Species is null)
        {
            throw new DeviceNotAssociatedException(deviceCode);
        }

        var species = plant.Species;

        // 1. Fuso de Brasília para garantir a hora exata
        var tzBrasilia = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "E. South America Standard Time" : "America/Sao_Paulo"
        );

        // 2. Converte o ReadAt do JSON (02:26:20-03:00) para o DateTime local de Brasília (02:26:20)
        var readAt = TimeZoneInfo.ConvertTime(request.ReadAt, tzBrasilia).DateTime;

        // 3. Pega o horário atual do servidor convertido para Brasília
        var nowBrasilia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzBrasilia);

        await using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var savedReadings = await _sensorReadingService.SaveReadingsAsync(
                plant.Id, device.Id, species, request.Readings, readAt);

            device.LastHeartbeatAt = nowBrasilia;

            // Proteção contra a sobrescrita do status Associated
            if (device.ConnectionStatus != ConnectionStatusEnum.Associated)
            {
                device.ConnectionStatus = ConnectionStatusEnum.Online;
            }

            var previousStatus = plant.HealthStatus;
            var newStatus = _plantHealthService.DetermineHealthStatus(species, savedReadings);

            plant.HealthStatus = newStatus;
            plant.UpdatedAt = nowBrasilia;

            await _db.SaveChangesAsync();

            await _notificationService.EvaluateAndCreateAsync(plant, species, savedReadings, previousStatus, newStatus);

            await transaction.CommitAsync();

            return new SensorReadingsResponse
            {
                DeviceCode = device.Code,
                PlantId = plant.Id,
                ReceivedAt = DateTimeOffset.UtcNow,
                ProcessedReadings = savedReadings.Count,
                PlantHealthStatus = newStatus.ToString()
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    /*
    public async Task<SensorReadingsResponse> ProcessReadingsAsync(string deviceCode, SensorReadingsRequest request)
    {
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.Code == deviceCode)
            ?? throw new DeviceNotFoundException(deviceCode);

        if (device.PlantId is null)
        {
            throw new DeviceNotAssociatedException(deviceCode);
        }

        var plant = await _db.Plants
            .Include(p => p.Species)
            .FirstOrDefaultAsync(p => p.Id == device.PlantId.Value);

        if (plant is null || plant.Species is null)
        {
            throw new DeviceNotAssociatedException(deviceCode);
        }

        var species = plant.Species;
        var readAt = request.ReadAt.UtcDateTime;


        await using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var savedReadings = await _sensorReadingService.SaveReadingsAsync(
                plant.Id, device.Id, species, request.Readings, readAt);

            device.LastHeartbeatAt = DateTime.Now;

            // Proteção contra a sobrescrita do status Associated
            if (device.ConnectionStatus != ConnectionStatusEnum.Associated)
            {
                device.ConnectionStatus = ConnectionStatusEnum.Online;
            }

            var previousStatus = plant.HealthStatus;
            var newStatus = _plantHealthService.DetermineHealthStatus(species, savedReadings);

            plant.HealthStatus = newStatus;
            plant.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            await _notificationService.EvaluateAndCreateAsync(plant, species, savedReadings, previousStatus, newStatus);

            await transaction.CommitAsync();

            return new SensorReadingsResponse
            {
                DeviceCode = device.Code,
                PlantId = plant.Id,
                ReceivedAt = DateTimeOffset.UtcNow,
                ProcessedReadings = savedReadings.Count,
                PlantHealthStatus = newStatus.ToString()
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        */
    /*
    await using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
    try
    {
        var savedReadings = await _sensorReadingService.SaveReadingsAsync(
            plant.Id, device.Id, species, request.Readings, readAt);

        device.LastHeartbeatAt = DateTime.Now;
        device.ConnectionStatus = ConnectionStatusEnum.Online;

        var previousStatus = plant.HealthStatus;
        var newStatus = _plantHealthService.DetermineHealthStatus(species, savedReadings);

        plant.HealthStatus = newStatus;
        plant.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        await _notificationService.EvaluateAndCreateAsync(plant, species, savedReadings, previousStatus, newStatus);

        await transaction.CommitAsync();

        return new SensorReadingsResponse
        {
            DeviceCode = device.Code,
            PlantId = plant.Id,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessedReadings = savedReadings.Count,
            PlantHealthStatus = newStatus.ToString()
        };
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
    */
}
