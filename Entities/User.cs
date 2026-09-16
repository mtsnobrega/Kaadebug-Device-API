namespace kaadebug_device_api.Entities;
/// <summary>
/// Mapeamento mínimo de User. A Device API não cria nem edita usuários,
/// só precisa do Id para gravar Notifications e, futuramente, poder checar
/// preferências de notificação (notifications_enabled / critical_alerts_only).
/// </summary>
public class User
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool NotificationsEnabled { get; set; }

    public bool CriticalAlertsOnly { get; set; }
}