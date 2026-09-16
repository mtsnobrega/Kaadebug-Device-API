using System.ComponentModel.DataAnnotations;

namespace kaadebug_device_api.Dtos.Device;
public class DeviceConnectRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string DeviceCode { get; set; } = string.Empty;
}
