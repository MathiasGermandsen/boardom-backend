using boardomapi.Models;
using Microsoft.EntityFrameworkCore;

namespace boardomapi.Database;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {
  }

  public DbSet<Device> Devices => Set<Device>();
  public DbSet<SensorData> SensorReadings => Set<SensorData>();
  public DbSet<Group> Groups => Set<Group>();
  public DbSet<DeviceGroup> DeviceGroups => Set<DeviceGroup>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Device>(entity =>
    {
      entity.ToTable("devices");
      entity.HasKey(e => e.DeviceId);
      entity.Property(e => e.DeviceId).HasColumnName("device_id");
      entity.Property(e => e.FriendlyName).HasColumnName("friendly_name").HasMaxLength(100);
      entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
      entity.HasQueryFilter(e => !e.IsDeleted);
    });

    modelBuilder.Entity<SensorData>(entity =>
    {
      entity.ToTable("data_arduino");
      entity.HasKey(e => e.PKey);
      entity.Property(e => e.PKey).HasColumnName("pkey").ValueGeneratedOnAdd();
      entity.Property(e => e.DeviceId).HasColumnName("deviceid");
      entity.Property(e => e.DateAdded).HasColumnName("dateadded").HasDefaultValueSql("NOW()");
      entity.Property(e => e.Temperature).HasColumnName("temperature");
      entity.Property(e => e.Humidity).HasColumnName("humidity");
      entity.Property(e => e.Pressure).HasColumnName("pressure");
      entity.Property(e => e.Light).HasColumnName("light");
      entity.Property(e => e.Moisture).HasColumnName("moisture");
    });

    modelBuilder.Entity<Group>(entity =>
    {
      entity.ToTable("groups");
      entity.HasKey(e => e.GroupId);
      entity.Property(e => e.GroupId).HasColumnName("group_id").ValueGeneratedOnAdd();
      entity.Property(e => e.GroupName).HasColumnName("group_name").HasMaxLength(100);
      entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
      entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
      entity.HasQueryFilter(e => !e.IsDeleted);
    });

    modelBuilder.Entity<DeviceGroup>(entity =>
    {
      entity.ToTable("device_groups");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
      entity.Property(e => e.GroupId).HasColumnName("group_id");
      entity.Property(e => e.DeviceId).HasColumnName("device_id");
      entity.Property(e => e.AddedAt).HasColumnName("added_at").HasDefaultValueSql("NOW()");

      entity.HasOne(e => e.Group)
          .WithMany()
          .HasForeignKey(e => e.GroupId);

      entity.HasOne(e => e.Device)
          .WithMany()
          .HasForeignKey(e => e.DeviceId)
          .IsRequired(false);
    });
  }
}
