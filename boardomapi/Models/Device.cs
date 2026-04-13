using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace boardomapi.Models;

[Table("devices")]
public class Device
{
  [Key]
  [Column("device_id")]
  public string DeviceId { get; set; } = string.Empty;

  [Required]
  [MaxLength(100)]
  [Column("friendly_name")]
  public string FriendlyName { get; set; } = string.Empty;

  [Column("is_deleted")]
  public bool IsDeleted { get; set; } = false;

  [Column("date_added")]
  public DateTime DateAdded { get; set; } = DateTime.UtcNow;

  [Column("last_heartbeat")]
  public DateTime LastHeartbeat { get; set; }

  [Column("user_id")]
  public string UserId { get; set; } = string.Empty;
}
