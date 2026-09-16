using System.ComponentModel.DataAnnotations;

namespace kaadebug_device_api.Dtos.Readings;

public class SensorReadingItem
{
    /// <summary>String recebida do ESP32 (ex: "SOIL_MOISTURE"); validada/convertida no service.</summary>
    [Required]
    public string SensorType { get; set; } = string.Empty;

    [Required]
    public decimal Value { get; set; }
}

public class SensorReadingsRequest
{
    [Required]
    public DateTimeOffset ReadAt { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "É necessário enviar ao menos uma leitura.")]
    public List<SensorReadingItem> Readings { get; set; } = new();
}