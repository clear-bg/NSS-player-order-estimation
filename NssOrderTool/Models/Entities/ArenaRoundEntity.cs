using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("ArenaRounds")]
  [Comment("アリーナセッション内の各ラウンド（局所的な対戦）の結果を管理するテーブル")]
  public class ArenaRoundEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("round_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("ラウンドのサロゲートキー（自動インクリメントID）")]
    public int Id { get; set; }

    [Column("session_id")]
    [Comment("紐づくアリーナセッションのID（ArenaSessionsテーブルの外部キー）")]
    public int SessionId { get; set; }

    [Column("round_number")]
    [Comment("セッション内でのラウンド進行番号（通常 1 〜 14）")]
    public int RoundNumber { get; set; } // 1 ～ 14

    // 0: 引き分け/無効, 1: Blue, 2: Orange
    [Column("winning_team")]
    [Comment("ラウンドの勝利チーム（0: 引き分け/無効, 1: Blueチーム, 2: Orangeチーム）")]
    public int WinningTeam { get; set; }

    [Column("is_deleted")]
    [Comment("論理削除フラグ（trueで削除済み）")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    [Comment("レコード作成日時")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [Comment("レコード最終更新日時")]
    public DateTime UpdatedAt { get; set; }

    // --- Navigation Properties ---
    [ForeignKey(nameof(SessionId))]
    public ArenaSessionEntity? Session { get; set; }
  }
}
