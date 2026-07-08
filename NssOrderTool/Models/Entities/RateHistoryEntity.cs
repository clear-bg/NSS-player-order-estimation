using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NssOrderTool.Models.Entities
{
  [Table("RateHistories")]
  [Comment("プレイヤーのレート変動履歴テーブル")]
  public class RateHistoryEntity
  {
    [Key]
    [Comment("自動採番ID")]
    public int Id { get; set; }

    [Required]
    [Comment("対象プレイヤーID")]
    public string PlayerId { get; set; } = "";

    [Required]
    [Comment("記録時点のレート値")]
    public double Rate { get; set; }

    [Required]
    [Comment("記録日時")]
    public DateTime RecordedAt { get; set; }

    [Column("season_id")]
    [Comment("所属シーズンID")]
    public int SeasonId { get; set; }
  }
}
