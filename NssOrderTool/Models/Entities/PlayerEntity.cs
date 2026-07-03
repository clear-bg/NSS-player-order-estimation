using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("Players")]
  [Comment("正規のプレイヤー基本情報および通算成績・最新レートを管理するテーブル")]
  public class PlayerEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("player_id")]
    [Comment("プレイヤーの一意な識別子（UUID文字列）")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Column("name")]
    [Comment("プレイヤーの正式な表示名")]
    public string? Name { get; set; }

    [Column("first_seen")]
    [Comment("システム内でプレイヤーが初めて観測された日時")]
    public DateTime FirstSeen { get; set; } = DateTime.Now;

    [Column("rate_mean")]
    [Comment("プレイヤーの現在の内部レート予測平均値（μ）")]
    public double RateMean { get; set; }

    [Column("rate_sigma")]
    [Comment("プレイヤーの現在のレート不確実性（σ）")]
    public double RateSigma { get; set; }

    [Column("is_deleted")]
    [Comment("論理削除フラグ（trueで削除済み）")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    [Comment("レコード作成日時")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [Comment("レコード最終更新日時")]
    public DateTime UpdatedAt { get; set; }

    [Column("is_active")]
    [Comment("プレイヤーが現在アクティブかどうかのフラグ")]
    public bool IsActive { get; set; } = true;

    [Column("last_played_at")]
    [Comment("最後に試合を行った日時")]
    public DateTime? LastPlayedAt { get; set; }

    [Column("total_matches")]
    [Comment("通算試合数")]
    public int TotalMatches { get; set; } = 0;

    [Column("total_wins")]
    [Comment("通算勝利数")]
    public int TotalWins { get; set; } = 0;

    [Column("memo")]
    [Comment("プレイヤーに関する自由記述のメモ")]
    public string Memo { get; set; } = string.Empty;
  }
}
