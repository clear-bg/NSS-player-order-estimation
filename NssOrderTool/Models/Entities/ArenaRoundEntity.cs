using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NssOrderTool.Models.Entities
{
  [Table("ArenaRounds")]
  public class ArenaRoundEntity
  {
    [Key]
    [Column("round_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("session_id")]
    public int SessionId { get; set; }

    [Column("round_number")]
    public int RoundNumber { get; set; } // 1 ～ 14

    // 0: 引き分け/無効, 1: Blue, 2: Orange
    [Column("winning_team")]
    public int WinningTeam { get; set; }

    // ナビゲーションプロパティ
    [ForeignKey(nameof(SessionId))]
    public ArenaSessionEntity? Session { get; set; }
  }
}
