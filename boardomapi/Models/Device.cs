using System.ComponentModel.DataAnnotations;

namespace boardomapi.Models;

public class Device
{
  [Key]
  public string DeviceId { get; set; } = string.Empty;

  [Required]
  [MaxLength(100)]
  public string FriendlyName { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
