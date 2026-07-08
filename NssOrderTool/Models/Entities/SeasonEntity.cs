using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NssOrderTool.Models.Entities
{
  [Table("Seasons")]
  [Comment("レート集計期間（シーズン）を管理するテーブル")]
  public class SeasonEntity
  {
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("自動採番ID")]
    public int Id { get; set; }

    [Column("name")]
    [Comment("シーズン表示名")]
    public string Name { get; set; } = string.Empty;

    [Column("start_date")]
    [Comment("開始日時")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    [Comment("終了日時（進行中はnull）")]
    public DateTime? EndDate { get; set; }

    [Column("is_active")]
    [Comment("進行中フラグ")]
    public bool IsActive { get; set; } = true;

    // --- Navigation Properties ---
    public List<ArenaSessionEntity> ArenaSessions { get; set; } = new();
  }
}
