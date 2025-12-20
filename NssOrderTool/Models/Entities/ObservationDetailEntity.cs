using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NssOrderTool.Models.Entities
{
  [Table("ObservationDetails")]
  public class ObservationDetailEntity
  {
    [Key]
    [Column("detail_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("observation_id")]
    public int ObservationId { get; set; }

    [Column("player_id")]
    public string PlayerId { get; set; } = "";

    // 何番目のプレイヤーか (0, 1, 2...)
    [Column("order_index")]
    public int OrderIndex { get; set; }

    // --- Navigation Properties ---
    [ForeignKey(nameof(ObservationId))]
    public ObservationEntity? Observation { get; set; }

    [ForeignKey(nameof(PlayerId))]
    public PlayerEntity? Player { get; set; }
  }
}
