using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace boardomapi.Models;

[Table("data_arduino")]
public class SensorData
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  [Column("pkey")]
  public int PKey { get; set; }

  [Required]
  [Column("device_id")]
  public string DeviceId { get; set; } = string.Empty;

  [Column("date_added")]
  public DateTime DateAdded { get; set; } = DateTime.UtcNow;

  [Column("temperature")]
  public double Temperature { get; set; }

  [Column("humidity")]
  public double Humidity { get; set; }

  [Column("pressure")]
  public double Pressure { get; set; }

  [Column("light")]
  public double Light { get; set; }

  [Column("moisture")]
  public double Moisture { get; set; }
}
