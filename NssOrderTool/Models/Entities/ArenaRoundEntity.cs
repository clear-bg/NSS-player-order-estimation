using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("ArenaRounds")]
  public class ArenaRoundEntity : ISoftDelete, ITimestamp
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

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // --- Navigation Properties ---
    [ForeignKey(nameof(SessionId))]
    public ArenaSessionEntity? Session { get; set; }
  }
}
