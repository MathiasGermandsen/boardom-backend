using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace boardomapi.Models;

public class Group
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int GroupId { get; set; }

  [Required]
  [MaxLength(100)]
  public string GroupName { get; set; } = string.Empty;

  public bool IsDeleted { get; set; } = false;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
