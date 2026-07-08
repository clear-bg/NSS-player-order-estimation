using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("ArenaParticipants")]
  [Comment("アリーナ参加者の結果を管理するテーブル")]
  public class ArenaParticipantEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("participant_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("自動採番ID")]
    public int Id { get; set; }

    [Column("session_id")]
    [Comment("アリーナセッションID")]
    public int SessionId { get; set; }

    [Column("player_id")]
    [Comment("参加プレイヤーID")]
    public string PlayerId { get; set; } = "";

    [Column("slot_index")]
    [Comment("座席番号（0〜7）")]
    public int SlotIndex { get; set; }

    [Column("win_count")]
    [Comment("獲得勝利数")]
    public int WinCount { get; set; }

    [Column("rank")]
    [Comment("最終順位")]
    public int Rank { get; set; }

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

    [ForeignKey(nameof(PlayerId))]
    public PlayerEntity? Player { get; set; }
  }
}
