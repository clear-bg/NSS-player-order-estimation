using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("Observations")]
  [Comment("プレイヤー出現順序の観測データを管理するテーブル")]
  public class ObservationEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("observation_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("自動採番ID")]
    public int Id { get; set; }

    [Column("observation_time")]
    [Comment("観測日時")]
    public DateTime ObservationTime { get; set; } = DateTime.Now;

    public List<ObservationDetailEntity> Details { get; set; } = new();

    [Column("is_deleted")]
    [Comment("論理削除フラグ")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    [Comment("作成日時")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [Comment("更新日時")]
    public DateTime UpdatedAt { get; set; }
  }
}
