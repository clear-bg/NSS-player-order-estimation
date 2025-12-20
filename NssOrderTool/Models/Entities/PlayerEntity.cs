using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("Players")] // DBのテーブル名を指定
  public class PlayerEntity : ISoftDelete
  {
    [Key] // 主キー
    [Column("player_id")]
    public string Id { get; set; } = "";

    [Column("name")]
    public string? Name { get; set; }

    [Column("first_seen")]
    public DateTime FirstSeen { get; set; } = DateTime.Now;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;
  }
}
