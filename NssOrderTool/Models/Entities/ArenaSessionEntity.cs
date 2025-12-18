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

    // 参加した8人のプレイヤーIDをカンマ区切りで保存 (順序 A, B, ... H)
    // 例: "PlayerA, PlayerB, ..., PlayerH"
    [Column("player_ids_csv")]
    public string PlayerIdsCsv { get; set; } = "";

    // リレーション (1対多)
    public List<ArenaRoundEntity> Rounds { get; set; } = new();
  }
}
