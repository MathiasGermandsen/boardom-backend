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
    modelBuilder.Entity<Device>()
      .HasQueryFilter(e => !e.IsDeleted);

    modelBuilder.Entity<Group>()
      .HasQueryFilter(e => !e.IsDeleted);

    modelBuilder.Entity<DeviceGroup>()
      .HasQueryFilter(e => e.Group != null && !e.Group.IsDeleted);
  }
}
