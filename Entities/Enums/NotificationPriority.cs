using NpgsqlTypes;

namespace kaadebug_device_api.Entities.Enums;

public enum NotificationPriority
{
    [PgName("LOW")]
    Low,
    [PgName("MEDIUM")]
    Medium,
    [PgName("HIGH")]
    High
}