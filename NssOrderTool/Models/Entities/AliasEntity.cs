using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("Aliases")]
  public class AliasEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("alias_name")]
    public string AliasName { get; set; } = "";

    [Column("target_player_id")]
    public string TargetPlayerId { get; set; } = "";

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
  }
}
