using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NssOrderTool.Models.Entities
{
  [Table("Seasons")]
  [Comment("レート計算や集計の期間区切りとなる「シーズン」の情報を管理するテーブル")]
  public class SeasonEntity
  {
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("シーズンのサロゲートキー（自動インクリメントID）")]
    public int Id { get; set; }

    [Column("name")]
    [Comment("シーズンの表示名（例: 'Season 1'、'2026 Spring' など）")]
    public string Name { get; set; } = string.Empty;

    [Column("start_date")]
    [Comment("シーズンの開始日時")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    [Comment("シーズンの終了日時（現在進行中のシーズンの場合は null）")]
    public DateTime? EndDate { get; set; }

    [Column("is_active")]
    [Comment("現在アクティブに進行しているシーズンかどうかのフラグ")]
    public bool IsActive { get; set; } = true;

    // --- Navigation Properties ---
    public List<ArenaSessionEntity> ArenaSessions { get; set; } = new();
  }
}
