using kaadebug_device_api.Data;
using kaadebug_device_api.Dtos.Device;
using kaadebug_device_api.Exceptions;
using kaadebug_device_api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace kaadebug_device_api.Services.Implementations;
public class DeviceConfigurationService : IDeviceConfigurationService
{
    private readonly KaaDebugDbContext _db;

    public DeviceConfigurationService(KaaDebugDbContext db)
    {
        _db = db;
    }

    public async Task<DeviceConfigurationResult> GetConfigurationAsync(string deviceCode)
    {
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.Code == deviceCode)
            ?? throw new DeviceNotFoundException(deviceCode);

        if (device.PlantId is null)
        {
            return new DeviceConfigurationResult
            {
                IsAssociated = false,
                NotAssociated = new DeviceNotAssociatedResponse
                {
                    DeviceId = device.Id,
                    DeviceCode = device.Code
                }
            };
        }

        var plant = await _db.Plants
            .Include(p => p.Species)
            .FirstOrDefaultAsync(p => p.Id == device.PlantId.Value);

        // Consistência: device aponta pra um plant_id que não existe mais.
        // Tratamos como "não associado" em vez de estourar erro pro ESP32.
        if (plant is null || plant.Species is null)
        {
            return new DeviceConfigurationResult
            {
                IsAssociated = false,
                NotAssociated = new DeviceNotAssociatedResponse
                {
                    DeviceId = device.Id,
                    DeviceCode = device.Code
                }
            };
        }

        var species = plant.Species;

        var configuration = new DeviceConfigurationResponse
        {
            DeviceId = device.Id,
            DeviceCode = device.Code,
            Associated = true,
            Plant = new PlantSummaryDto { PlantId = plant.Id, Name = plant.Name },
            Species = new SpeciesSummaryDto { SpeciesId = species.Id, Name = species.Name },
            MonitoringParameters = new MonitoringParametersDto
            {
                SoilMoisture = new RangeDto { Min = species.SoilMoistureMin, Max = species.SoilMoistureMax },
                AirHumidity = new RangeDto { Min = species.AirHumidityMin, Max = species.AirHumidityMax },
                Temperature = new RangeDto { Min = species.TemperatureMin, Max = species.TemperatureMax }
            }
        };

        return new DeviceConfigurationResult
        {
            IsAssociated = true,
            Configuration = configuration
        };
    }
}