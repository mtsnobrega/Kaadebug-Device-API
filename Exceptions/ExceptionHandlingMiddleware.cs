using kaadebug_device_api.Dtos.Common;
using System.Text.Json;

namespace kaadebug_device_api.Exceptions;
/// <summary>
/// Middleware central: captura as exceções de domínio lançadas pelos services
/// e traduz para o código HTTP e formato de erro (ApiErrorResponse) adequados,
/// para que os Controllers não precisem de try/catch repetido em cada endpoint.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DeviceNotFoundException ex)
        {
            await WriteError(context, StatusCodes.Status404NotFound, ex.Message, "DEVICE_NOT_FOUND");
        }
        catch (DeviceNotAssociatedException ex)
        {
            await WriteError(context, StatusCodes.Status409Conflict, ex.Message, "DEVICE_NOT_ASSOCIATED");
        }
        catch (InvalidSensorReadingException ex)
        {
            await WriteError(context, StatusCodes.Status400BadRequest, ex.Message, "INVALID_SENSOR_READING");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado processando {Path}", context.Request.Path);
            await WriteError(context, StatusCodes.Status500InternalServerError,
                "Ocorreu um erro inesperado ao processar a requisição.", "INTERNAL_ERROR");
        }
    }

    private static async Task WriteError(HttpContext context, int statusCode, string message, string errorCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var payload = new ApiErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            ErrorCode = errorCode
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseDeviceApiExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}