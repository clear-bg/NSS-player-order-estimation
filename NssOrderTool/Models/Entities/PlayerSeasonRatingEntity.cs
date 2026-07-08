using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("PlayerSeasonRatings")]
  [Comment("シーズン別レート・成績のスナップショットを管理するテーブル")]
  public class PlayerSeasonRatingEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("自動採番ID")]
    public int Id { get; set; }

    [Column("player_id")]
    [Comment("対象プレイヤーID")]
    public string PlayerId { get; set; } = string.Empty;

    [Column("season_id")]
    [Comment("対象シーズンID")]
    public int SeasonId { get; set; }

    [Column("rate_mean")]
    [Comment("シーズン内レート平均値（μ）")]
    public double RateMean { get; set; }

    [Column("rate_sigma")]
    [Comment("シーズン内レート不確実性（σ）")]
    public double RateSigma { get; set; }

    [Column("total_matches")]
    [Comment("シーズン内試合数")]
    public int TotalMatches { get; set; } = 0;

    [Column("total_wins")]
    [Comment("シーズン内勝利数")]
    public int TotalWins { get; set; } = 0;

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
    [ForeignKey(nameof(PlayerId))]
    public PlayerEntity? Player { get; set; }

    [ForeignKey(nameof(SeasonId))]
    public SeasonEntity? Season { get; set; }
  }
}
