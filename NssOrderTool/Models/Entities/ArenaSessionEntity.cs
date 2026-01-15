using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("ArenaSessions")]
  public class ArenaSessionEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("session_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // --- Navigation Properties ---

    // 参加者リスト
    public List<ArenaParticipantEntity> Participants { get; set; } = new();

    // リレーション (1対多)
    public List<ArenaRoundEntity> Rounds { get; set; } = new();

    [NotMapped]
    public string WinningTeam
    {
      get
      {
        if (Rounds == null || Rounds.Count == 0) return "-";

        // 修正: 文字列 "Blue" ではなく 数値 1 で比較
        var blueWins = Rounds.Count(r => r.WinningTeam == 1);

        // 修正: 文字列 "Orange" ではなく 数値 2 で比較
        var orangeWins = Rounds.Count(r => r.WinningTeam == 2);

        if (blueWins > orangeWins) return "Blue";
        if (orangeWins > blueWins) return "Orange";
        return "Draw";
      }
    }
  }
}
