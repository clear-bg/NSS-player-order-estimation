using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NssOrderTool.Models.Entities
{
  [Table("ArenaSessions")]
  public class ArenaSessionEntity
  {
    [Key]
    [Column("session_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // 参加者リスト
    public List<ArenaParticipantEntity> Participants { get; set; } = new();

    // リレーション (1対多)
    public List<ArenaRoundEntity> Rounds { get; set; } = new();
  }
}
