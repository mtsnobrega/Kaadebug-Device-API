using NpgsqlTypes;
using System.Text.Json.Serialization;

namespace kaadebug_device_api.Entities.Enums;

/// <summary>
/// Tipos de sensor suportados. O projeto não utiliza luminosidade (LUMINOSITY) -
/// não adicionar esse valor aqui.
/// </summary>

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SensorType
{
    [PgName("SOIL_MOISTURE")]
    [JsonPropertyName("SOIL_MOISTURE")]
    SoilMoisture,
    [PgName("AIR_HUMIDITY")]
    [JsonPropertyName("AIR_HUMIDITY")]
    AirHumidity,
    [PgName("TEMPERATURE")]
    [JsonPropertyName("TEMPERATURE")]
    Temperature
}