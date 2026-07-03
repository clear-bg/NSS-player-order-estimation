using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("Observations")]
  [Comment("プレイヤーの出現順序などの観測データ（親レコード）を管理するテーブル")]
  public class ObservationEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("observation_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("観測データのサロゲートキー（自動インクリメントID）")]
    public int Id { get; set; }

    [Column("observation_time")]
    [Comment("観測が実行・記録された日時")]
    public DateTime ObservationTime { get; set; } = DateTime.Now;

    public List<ObservationDetailEntity> Details { get; set; } = new();

    [Column("is_deleted")]
    [Comment("論理削除フラグ（trueで削除済み）")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    [Comment("レコード作成日時")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [Comment("レコード最終更新日時")]
    public DateTime UpdatedAt { get; set; }
  }
}
