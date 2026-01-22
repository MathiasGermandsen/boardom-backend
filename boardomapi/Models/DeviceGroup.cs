using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace boardomapi.Models;

public class DeviceGroup
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [Required]
  public int GroupId { get; set; }

  [Required]
  public string DeviceId { get; set; } = string.Empty;

  public DateTime AddedAt { get; set; } = DateTime.UtcNow;

  // Navigation properties
  public Group? Group { get; set; }
  public Device? Device { get; set; }
}
