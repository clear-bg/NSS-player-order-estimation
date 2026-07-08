using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("ObservationDetails")]
  [Comment("観測データの明細（プレイヤー順序）を管理するテーブル")]
  public class ObservationDetailEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("detail_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("自動採番ID")]
    public int Id { get; set; }

    [Column("observation_id")]
    [Comment("親観測データID")]
    public int ObservationId { get; set; }

    [Column("player_id")]
    [Comment("観測プレイヤーID")]
    public string PlayerId { get; set; } = "";

    [Column("order_index")]
    [Comment("出現順序（0,1,2...）")]
    public int OrderIndex { get; set; }

    // --- Navigation Properties ---
    [ForeignKey(nameof(ObservationId))]
    public ObservationEntity? Observation { get; set; }

    [ForeignKey(nameof(PlayerId))]
    public PlayerEntity? Player { get; set; }

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
