using kaadebug_device_api.Entities.Enums;

namespace kaadebug_device_api.Entities;

public class Plant
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid SpeciesId { get; set; }
    public Species? Species { get; set; }

    public Guid? DeviceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    public HealthStatus HealthStatus { get; set; }

    public string? StatusReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
