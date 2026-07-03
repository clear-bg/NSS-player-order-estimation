using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("PlayerSeasonRatings")]
  [Comment("プレイヤーのシーズンごとのレート情報および成績（スナップショット）を管理するテーブル")]
  public class PlayerSeasonRatingEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("シーズン別レート情報のサロゲートキー（自動インクリメントID）")]
    public int Id { get; set; }

    [Column("player_id")]
    [Comment("対象プレイヤーのID（Playersテーブルの外部キー）")]
    public string PlayerId { get; set; } = string.Empty;

    [Column("season_id")]
    [Comment("対象シーズンのID（Seasonsテーブルの外部キー）")]
    public int SeasonId { get; set; }

    [Column("rate_mean")]
    [Comment("このシーズンにおけるレート予測平均値（μ）")]
    public double RateMean { get; set; }

    [Column("rate_sigma")]
    [Comment("このシーズンにおけるレート不確実性（σ）")]
    public double RateSigma { get; set; }

    [Column("total_matches")]
    [Comment("このシーズンでの累計試合数")]
    public int TotalMatches { get; set; } = 0;

    [Column("total_wins")]
    [Comment("このシーズンでの累計勝利数")]
    public int TotalWins { get; set; } = 0;

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
    [ForeignKey(nameof(PlayerId))]
    public PlayerEntity? Player { get; set; }

    [ForeignKey(nameof(SeasonId))]
    public SeasonEntity? Season { get; set; }
  }
}
