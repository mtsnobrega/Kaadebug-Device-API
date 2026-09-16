using kaadebug_device_api.Data;
using kaadebug_device_api.Entities.Enums;
using kaadebug_device_api.Exceptions;
using kaadebug_device_api.Services.Implementations;
using kaadebug_device_api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.NameTranslation;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// O NpgsqlSnakeCaseNameTranslator converte PascalCase (C#) para snake_case (PostgreSQL)
// Exemplo: 'HealthStatus.GoodHealth' vira 'good_health'
var snakeCaseTranslator = new NpgsqlSnakeCaseNameTranslator();

// Configuração direta e integrada ao EF Core
builder.Services.AddDbContext<KaaDebugDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Ao mapear aqui, tanto o EF Core quanto o driver do Postgres (Npgsql) ficam cientes
        npgsqlOptions.MapEnum<HealthStatus>("health_status", nameTranslator: snakeCaseTranslator);
        npgsqlOptions.MapEnum<ConnectionStatus>("connection_status", nameTranslator: snakeCaseTranslator);
        npgsqlOptions.MapEnum<SensorType>("sensor_type", nameTranslator: snakeCaseTranslator);
        npgsqlOptions.MapEnum<NotificationPriority>("notification_priority", nameTranslator: snakeCaseTranslator);
    })
);

// --- Services (injeção direta do DbContext, sem camada de Repository) ---
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IDeviceConfigurationService, DeviceConfigurationService>();
builder.Services.AddScoped<IPlantMonitoringService, PlantMonitoringService>();
builder.Services.AddScoped<ISensorReadingService, SensorReadingService>();
builder.Services.AddScoped<IPlantHealthService, PlantHealthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// --- Controllers ---
builder.Services.AddControllers();

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Swagger ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "KaaDebug Device API v1");
        options.RoutePrefix = "swagger";
    });
}
// HTTPS obrigatório em produção (seção 17 do escopo).
app.UseHttpsRedirection();

// Middleware central de tratamento de exceções de domínio -> códigos HTTP.
app.UseDeviceApiExceptionHandling();

app.UseAuthorization();

app.MapControllers();

app.Run();
