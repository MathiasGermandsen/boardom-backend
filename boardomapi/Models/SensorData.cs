using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace boardomapi.Models;

public class SensorData
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int PKey { get; set; }

  [Required]
  public string DeviceId { get; set; } = string.Empty;

  public DateTime DateAdded { get; set; } = DateTime.UtcNow;

  public double Temperature { get; set; }

  public double Humidity { get; set; }

  public double Pressure { get; set; }

  public double Light { get; set; }

  public double Moisture { get; set; }

  // Navigation property
  public Device? Device { get; set; }
}
