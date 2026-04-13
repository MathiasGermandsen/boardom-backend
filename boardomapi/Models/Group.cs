using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace boardomapi.Models;

[Table("groups")]
public class Group
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  [Column("group_id")]
  public int GroupId { get; set; }

  [Required]
  [MaxLength(100)]
  [Column("group_name")]
  public string GroupName { get; set; } = string.Empty;

  [Column("is_deleted")]
  public bool IsDeleted { get; set; } = false;

  [Column("date_added")]
  public DateTime DateAdded { get; set; } = DateTime.UtcNow;

  [Column("user_id")]
  public string UserId { get; set; } = string.Empty;
}
