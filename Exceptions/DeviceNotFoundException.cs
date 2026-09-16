namespace kaadebug_device_api.Exceptions;

/// <summary>Lançada quando o deviceCode informado não existe no banco. Mapeada para HTTP 404.</summary>
public class DeviceNotFoundException : Exception
{
    public DeviceNotFoundException(string deviceCode)
        : base($"Dispositivo com código '{deviceCode}' não foi encontrado.") { }
}

/// <summary>
/// Lançada quando uma operação exige um device associado a uma planta
/// (ex: envio de leituras) mas o device ainda não está associado. Mapeada para HTTP 409.
/// </summary>
public class DeviceNotAssociatedException : Exception
{
    public DeviceNotAssociatedException(string deviceCode)
        : base($"Dispositivo com código '{deviceCode}' ainda não está associado a uma planta.") { }
}

/// <summary>
/// Lançada quando um sensorType enviado pelo ESP32 não corresponde a nenhum
/// valor válido do enum SensorType. Mapeada para HTTP 400.
/// </summary>
public class InvalidSensorReadingException : Exception
{
    public InvalidSensorReadingException(string message) : base(message) { }
}