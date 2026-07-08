using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("ArenaRounds")]
  [Comment("アリーナラウンドの結果を管理するテーブル")]
  public class ArenaRoundEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("round_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("自動採番ID")]
    public int Id { get; set; }

    [Column("session_id")]
    [Comment("アリーナセッションID")]
    public int SessionId { get; set; }

    [Column("round_number")]
    [Comment("ラウンド番号（1〜14）")]
    public int RoundNumber { get; set; } // 1 ～ 14

    // 0: 引き分け/無効, 1: Blue, 2: Orange
    [Column("winning_team")]
    [Comment("勝利チーム（0:無効 1:Blue 2:Orange）")]
    public int WinningTeam { get; set; }

    [Column("is_deleted")]
    [Comment("論理削除フラグ")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    [Comment("作成日時")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [Comment("更新日時")]
    public DateTime UpdatedAt { get; set; }

    // --- Navigation Properties ---
    [ForeignKey(nameof(SessionId))]
    public ArenaSessionEntity? Session { get; set; }
  }
}
