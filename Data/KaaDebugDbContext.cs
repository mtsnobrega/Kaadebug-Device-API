using kaadebug_device_api.Entities;
using kaadebug_device_api.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace kaadebug_device_api.Data;
/// <summary>
/// DbContext da Device API. Mapeia apenas as tabelas que esta API usa
/// (não inclui diagnosis_results, que é exclusiva da BFF/app mobile).
/// Os enums são persistidos como texto (HasConversion&lt;string&gt;) para
/// bater com os enums nativos do Postgres já existentes no banco.
/// </summary>
public class KaaDebugDbContext : DbContext
{
    public KaaDebugDbContext(DbContextOptions<KaaDebugDbContext> options) : base(options) { }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<Species> Species => Set<Species>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<Notification> Notifications => Set<Notification>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tipos ENUM nativos utilizados pelo PostgreSQL.
        modelBuilder.HasPostgresEnum<HealthStatus>(
            "health_status");

        modelBuilder.HasPostgresEnum<ConnectionStatus>(
            "connection_status");

        modelBuilder.HasPostgresEnum<SensorType>(
            "sensor_type");

        modelBuilder.HasPostgresEnum<NotificationPriority>(
            "notification_priority");

        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("devices");

            entity.HasKey(d => d.Id);

            entity.Property(d => d.Id)
                .HasColumnName("id");

            entity.Property(d => d.Code)
                .HasColumnName("code")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(d => d.ConnectionStatus)
                .HasColumnName("connection_status")
                .HasColumnType("connection_status");

            entity.Property(d => d.LastHeartbeatAt)
                .HasColumnName("last_heartbeat_at");

            entity.Property(d => d.RegisteredAt)
                .HasColumnName("registered_at");

            // Diferente das demais colunas (snake_case), esta foi criada
            // como "PlantId" e precisa do nome exato no PostgreSQL.
            entity.Property(d => d.PlantId)
                .HasColumnName("PlantId");

            entity.Property(d => d.UserId)
                .HasColumnName("user_id");

            entity.HasIndex(d => d.Code)
                .IsUnique();

            entity.HasOne(d => d.Plant)
                .WithMany()
                .HasForeignKey(d => d.PlantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Plant>(entity =>
        {
            entity.ToTable("plants");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id)
                .HasColumnName("id");

            entity.Property(p => p.UserId)
                .HasColumnName("user_id");

            entity.Property(p => p.SpeciesId)
                .HasColumnName("species_id");

            entity.Property(p => p.DeviceId)
                .HasColumnName("device_id");

            entity.Property(p => p.Name)
                .HasColumnName("name");

            entity.Property(p => p.PhotoUrl)
                .HasColumnName("photo_url");

            entity.Property(p => p.HealthStatus)
                .HasColumnName("health_status")
                .HasColumnType("health_status");

            entity.Property(p => p.StatusReason)
                .HasColumnName("status_reason");

            entity.Property(p => p.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(p => p.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(p => p.Species)
                .WithMany()
                .HasForeignKey(p => p.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Species>(entity =>
        {
            entity.ToTable("species");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Id)
                .HasColumnName("id");

            entity.Property(s => s.Name)
                .HasColumnName("name");

            entity.Property(s => s.PhotoUrl)
                .HasColumnName("photo_url");

            entity.Property(s => s.SoilMoistureMin)
                .HasColumnName("soil_moisture_min")
                .HasColumnType("numeric(10,2)");

            entity.Property(s => s.SoilMoistureMax)
                .HasColumnName("soil_moisture_max")
                .HasColumnType("numeric(10,2)");

            entity.Property(s => s.AirHumidityMin)
                .HasColumnName("air_humidity_min")
                .HasColumnType("numeric(10,2)");

            entity.Property(s => s.AirHumidityMax)
                .HasColumnName("air_humidity_max")
                .HasColumnType("numeric(10,2)");

            entity.Property(s => s.TemperatureMin)
                .HasColumnName("temperature_min")
                .HasColumnType("numeric(10,2)");

            entity.Property(s => s.TemperatureMax)
                .HasColumnName("temperature_max")
                .HasColumnType("numeric(10,2)");

            entity.Property(s => s.CareInfo)
                .HasColumnName("care_info");

            entity.Property(s => s.IrrigationIntervalHours)
                .HasColumnName("irrigation_interval_hours");

            entity.Property(s => s.ReadingFrequency)
                .HasColumnName("reading_frequency");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                .HasColumnName("id");

            entity.Property(u => u.Name)
                .HasColumnName("name");

            entity.Property(u => u.Email)
                .HasColumnName("email");

            entity.Property(u => u.NotificationsEnabled)
                .HasColumnName("notifications_enabled");

            entity.Property(u => u.CriticalAlertsOnly)
                .HasColumnName("critical_alerts_only");
        });

        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.ToTable("sensor_readings");

            entity.HasKey(r => r.Id);

            entity.Property(r => r.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(r => r.PlantId)
                .HasColumnName("plant_id");

            entity.Property(r => r.DeviceId)
                .HasColumnName("device_id");

            entity.Property(r => r.SensorType)
                .HasColumnName("sensor_type")
                .HasColumnType("sensor_type");

            entity.Property(r => r.Value)
                .HasColumnName("value")
                .HasColumnType("numeric(10,2)");

            entity.Property(r => r.IsWithinIdealRange)
                .HasColumnName("is_within_ideal_range");

            entity.Property(r => r.ReadAt)
                .HasColumnName("read_at");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            entity.HasKey(n => n.Id);

            entity.Property(n => n.Id)
                .HasColumnName("id");

            entity.Property(n => n.UserId)
                .HasColumnName("user_id");

            entity.Property(n => n.PlantId)
                .HasColumnName("plant_id");

            entity.Property(n => n.Message)
                .HasColumnName("message");

            entity.Property(n => n.Priority)
                .HasColumnName("priority")
                .HasColumnType("notification_priority");

            entity.Property(n => n.IsRead)
                .HasColumnName("is_read");

            entity.Property(n => n.CreatedAt)
                .HasColumnName("created_at");
        });
    }









    /*
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("devices");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).HasColumnName("id");
            entity.Property(d => d.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(d => d.ConnectionStatus).HasColumnName("connection_status").HasColumnType("connection_status");
            entity.Property(d => d.LastHeartbeatAt).HasColumnName("last_heartbeat_at");
            entity.Property(d => d.RegisteredAt).HasColumnName("registered_at");
            // Diferente das demais colunas (snake_case), esta foi criada como "PlantId"
            // (mista, entre aspas) - precisa do HasColumnName exato para o Postgres achar a coluna.
            entity.Property(d => d.PlantId).HasColumnName("PlantId");
            entity.Property(d => d.UserId).HasColumnName("user_id");

            entity.HasIndex(d => d.Code).IsUnique();

            entity.HasOne(d => d.Plant)
                .WithMany()
                .HasForeignKey(d => d.PlantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Plant>(entity =>
        {
            entity.ToTable("plants");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.UserId).HasColumnName("user_id");
            entity.Property(p => p.SpeciesId).HasColumnName("species_id");
            entity.Property(p => p.DeviceId).HasColumnName("device_id");
            entity.Property(p => p.Name).HasColumnName("name");
            entity.Property(p => p.PhotoUrl).HasColumnName("photo_url");
            entity.Property(p => p.HealthStatus).HasColumnName("health_status").HasColumnType("health_status");
            entity.Property(p => p.StatusReason).HasColumnName("status_reason");
            entity.Property(p => p.CreatedAt).HasColumnName("created_at");
            entity.Property(p => p.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(p => p.Species)
                .WithMany()
                .HasForeignKey(p => p.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Species>(entity =>
        {
            entity.ToTable("species");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).HasColumnName("id");
            entity.Property(s => s.Name).HasColumnName("name");
            entity.Property(s => s.PhotoUrl).HasColumnName("photo_url");
            entity.Property(s => s.SoilMoistureMin).HasColumnName("soil_moisture_min").HasColumnType("numeric(10,2)");
            entity.Property(s => s.SoilMoistureMax).HasColumnName("soil_moisture_max").HasColumnType("numeric(10,2)");
            entity.Property(s => s.AirHumidityMin).HasColumnName("air_humidity_min").HasColumnType("numeric(10,2)");
            entity.Property(s => s.AirHumidityMax).HasColumnName("air_humidity_max").HasColumnType("numeric(10,2)");
            entity.Property(s => s.TemperatureMin).HasColumnName("temperature_min").HasColumnType("numeric(10,2)");
            entity.Property(s => s.TemperatureMax).HasColumnName("temperature_max").HasColumnType("numeric(10,2)");
            entity.Property(s => s.CareInfo).HasColumnName("care_info");
            entity.Property(s => s.IrrigationIntervalHours).HasColumnName("irrigation_interval_hours");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.Name).HasColumnName("name");
            entity.Property(u => u.Email).HasColumnName("email");
            entity.Property(u => u.NotificationsEnabled).HasColumnName("notifications_enabled");
            entity.Property(u => u.CriticalAlertsOnly).HasColumnName("critical_alerts_only");
        });

        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.ToTable("sensor_readings");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(r => r.PlantId).HasColumnName("plant_id");
            entity.Property(r => r.DeviceId).HasColumnName("device_id");
            entity.Property(r => r.SensorType).HasColumnName("sensor_type").HasColumnType("sensor_type");
            entity.Property(r => r.Value).HasColumnName("value").HasColumnType("numeric(10,2)");
            entity.Property(r => r.IsWithinIdealRange).HasColumnName("is_within_ideal_range");
            entity.Property(r => r.ReadAt).HasColumnName("read_at");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Id).HasColumnName("id");
            entity.Property(n => n.UserId).HasColumnName("user_id");
            entity.Property(n => n.PlantId).HasColumnName("plant_id");
            entity.Property(n => n.Message).HasColumnName("message");
            entity.Property(n => n.Priority).HasColumnName("priority").HasColumnType("notification_priority");
            entity.Property(n => n.IsRead).HasColumnName("is_read");
            entity.Property(n => n.CreatedAt).HasColumnName("created_at");
        });
    }
    */
}