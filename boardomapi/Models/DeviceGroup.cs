using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace boardomapi.Models;

[Table("device_groups")]
public class DeviceGroup
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  [Column("id")]
  public int Id { get; set; }

  [Required]
  [Column("group_id")]
  [ForeignKey(nameof(Group))]
  public int GroupId { get; set; }

  [Column("device_id")]
  [ForeignKey(nameof(Device))]
  public string? DeviceId { get; set; }

  [Column("added_at")]
  public DateTime AddedAt { get; set; } = DateTime.UtcNow;

  // Navigation properties
  public Group? Group { get; set; }
  public Device? Device { get; set; }
}
