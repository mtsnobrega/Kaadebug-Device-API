using kaadebug_device_api.Entities.Enums;

namespace kaadebug_device_api.Entities;

/// <summary>
/// Representa um dispositivo ESP32 previamente cadastrado no banco.
/// A Device API nunca cria um Device; ela só lê e atualiza campos de estado
/// de conexão (ConnectionStatus, LastHeartbeatAt).
/// </summary>
public class Device
{
    public Guid Id { get; set; }

    /// <summary>Código fixo gravado no firmware do ESP32 (ex: "ESP32-0001").</summary>
    public string Code { get; set; } = string.Empty;

    public ConnectionStatus ConnectionStatus { get; set; }

    public DateTime? LastHeartbeatAt { get; set; }

    public DateTime RegisteredAt { get; set; }

    /// <summary>Planta associada. Associação é feita pelo BFF/app mobile, não por esta API.</summary>
    public Guid? PlantId { get; set; }
    public Plant? Plant { get; set; }

    public Guid? UserId { get; set; }
}