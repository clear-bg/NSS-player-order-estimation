using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NssOrderTool.Models.Entities
{
  [Table("Players")] // DBのテーブル名を指定
  public class PlayerEntity
  {
    [Key] // 主キー
    [Column("player_id")]
    public string Id { get; set; } = "";

    [Column("name")]
    public string? Name { get; set; }

    [Column("first_seen")]
    public DateTime FirstSeen { get; set; } = DateTime.Now;
  }
}