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
        var readAt = request.ReadAt.UtcDateTime;

        await using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var savedReadings = await _sensorReadingService.SaveReadingsAsync(
                plant.Id, device.Id, species, request.Readings, readAt);

            device.LastHeartbeatAt = DateTime.UtcNow;
            device.ConnectionStatus = ConnectionStatusEnum.Online;

            var previousStatus = plant.HealthStatus;
            var newStatus = _plantHealthService.DetermineHealthStatus(species, savedReadings);

            plant.HealthStatus = newStatus;
            plant.UpdatedAt = DateTime.UtcNow;

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
}