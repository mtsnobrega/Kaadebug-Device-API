using kaadebug_device_api.Entities.Enums;

namespace kaadebug_device_api.Entities;

public class Notification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid PlantId { get; set; }

    public string Message { get; set; } = string.Empty;

    public NotificationPriority Priority { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}
