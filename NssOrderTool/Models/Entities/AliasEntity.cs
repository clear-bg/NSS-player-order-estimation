using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NssOrderTool.Models.Entities
{
  [Table("Aliases")]
  public class AliasEntity
  {
    [Key]
    [Column("alias_name")]
    public string AliasName { get; set; } = "";

    [Column("target_player_id")]
    public string TargetPlayerId { get; set; } = "";
  }
}
