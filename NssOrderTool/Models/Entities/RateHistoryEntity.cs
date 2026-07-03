using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NssOrderTool.Models.Entities
{
  [Table("RateHistories")]
  [Comment("プレイヤーのレート変動履歴を時系列で記録するテーブル")]
  public class RateHistoryEntity
  {
    [Key]
    [Comment("レート履歴のサロゲートキー（自動インクリメントID）")]
    public int Id { get; set; }

    [Required]
    [Comment("対象プレイヤーのID（Playersテーブルの外部キー）")]
    public string PlayerId { get; set; } = "";

    [Required]
    [Comment("記録時点での計算済みレート値")]
    public double Rate { get; set; }

    [Required]
    [Comment("レートが変動・記録された日時")]
    public DateTime RecordedAt { get; set; }

    [Column("season_id")]
    [Comment("この履歴が属するシーズンのID（Seasonsテーブルの外部キー）")]
    public int SeasonId { get; set; }
  }
}
