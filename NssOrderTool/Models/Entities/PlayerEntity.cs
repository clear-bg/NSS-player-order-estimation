using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("Players")]
  [Comment("プレイヤーの基本情報と成績を管理するテーブル")]
  public class PlayerEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("player_id")]
    [Comment("プレイヤーID（UUID）")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Column("name")]
    [Comment("表示名")]
    public string? Name { get; set; }

    [Column("first_seen")]
    [Comment("初回観測日時")]
    public DateTime FirstSeen { get; set; } = DateTime.Now;

    [Column("rate_mean")]
    [Comment("現在のレート平均値（μ）")]
    public double RateMean { get; set; }

    [Column("rate_sigma")]
    [Comment("現在のレート不確実性（σ）")]
    public double RateSigma { get; set; }

    [Column("is_deleted")]
    [Comment("論理削除フラグ")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    [Comment("作成日時")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [Comment("更新日時")]
    public DateTime UpdatedAt { get; set; }

    [Column("is_active")]
    [Comment("アクティブフラグ")]
    public bool IsActive { get; set; } = true;

    [Column("last_played_at")]
    [Comment("最終対戦日時")]
    public DateTime? LastPlayedAt { get; set; }

    [Column("total_matches")]
    [Comment("通算試合数")]
    public int TotalMatches { get; set; } = 0;

    [Column("total_wins")]
    [Comment("通算勝利数")]
    public int TotalWins { get; set; } = 0;

    [Column("memo")]
    [Comment("自由記述メモ")]
    public string Memo { get; set; } = string.Empty;
  }
}
