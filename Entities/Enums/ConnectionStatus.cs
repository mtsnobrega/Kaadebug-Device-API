using NpgsqlTypes;

namespace kaadebug_device_api.Entities.Enums;
/// <summary>
/// Estados possíveis de conexão de um dispositivo.
/// A Device API só transiciona entre ONLINE e OFFLINE.
/// UNASSOCIATED, ASSOCIATED e NOTFOUND são tratados fora desta API (BFF/app mobile),
/// e NOTFOUND nunca é persistido (é apenas um HTTP 404 quando o deviceCode não existe).
/// </summary>
public enum ConnectionStatus
{
    [PgName("ONLINE")]
    Online,
    [PgName("OFFLINE")]
    Offline,
    [PgName("UNASSOCIATED")]
    Unassociated,
    [PgName("ASSOCIATED")]
    Associated,
    [PgName("NOTFOUND")]
    NotFound
}
