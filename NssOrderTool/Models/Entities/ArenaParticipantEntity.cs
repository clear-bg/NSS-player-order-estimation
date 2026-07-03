using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("ArenaParticipants")]
  [Comment("アリーナセッションに参加する各プレイヤーの情報と最終結果（座席、勝利数、順位など）を管理するテーブル")]
  public class ArenaParticipantEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("participant_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("参加情報のサロゲートキー（自動インクリメントID）")]
    public int Id { get; set; }

    [Column("session_id")]
    [Comment("紐づくアリーナセッションのID（ArenaSessionsテーブルの外部キー）")]
    public int SessionId { get; set; }

    [Column("player_id")]
    [Comment("参加したプレイヤーのID（Playersテーブルの外部キー）")]
    public string PlayerId { get; set; } = "";

    [Column("slot_index")]
    [Comment("セッション内での座席番号・配置インデックス（通常 0〜7）")]
    public int SlotIndex { get; set; }

    [Column("win_count")]
    [Comment("このセッションで獲得した最終的な勝利数")]
    public int WinCount { get; set; }

    [Column("rank")]
    [Comment("このセッションでの最終順位")]
    public int Rank { get; set; }

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

    [ForeignKey(nameof(PlayerId))]
    public PlayerEntity? Player { get; set; }
  }
}
