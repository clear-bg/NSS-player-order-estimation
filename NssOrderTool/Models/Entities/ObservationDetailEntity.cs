using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("ObservationDetails")]
  [Comment("1回の観測データに含まれる個々のプレイヤーとその順序（子レコード）を管理するテーブル")]
  public class ObservationDetailEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("detail_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("観測詳細データのサロゲートキー（自動インクリメントID）")]
    public int Id { get; set; }

    [Column("observation_id")]
    [Comment("紐づく親観測データのID（Observationsテーブルの外部キー）")]
    public int ObservationId { get; set; }

    [Column("player_id")]
    [Comment("観測されたプレイヤーのID（Playersテーブルの外部キー）")]
    public string PlayerId { get; set; } = "";

    [Column("order_index")]
    [Comment("観測されたプレイヤーの順番・配置インデックス（0, 1, 2...）")]
    public int OrderIndex { get; set; }

    // --- Navigation Properties ---
    [ForeignKey(nameof(ObservationId))]
    public ObservationEntity? Observation { get; set; }

    [ForeignKey(nameof(PlayerId))]
    public PlayerEntity? Player { get; set; }

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
