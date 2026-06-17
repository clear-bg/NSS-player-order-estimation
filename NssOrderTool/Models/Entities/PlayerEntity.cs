using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("Players")] // DBのテーブル名を指定
  public class PlayerEntity : ISoftDelete, ITimestamp
  {
    [Key] // 主キー
    [Column("player_id")]
    public string Id { get; set; } = Guid.NewGuid().ToString(); // UUIDを自動生成

    [Column("name")]
    public string? Name { get; set; }

    [Column("first_seen")]
    public DateTime FirstSeen { get; set; } = DateTime.Now;

    [Column("rate_mean")]
    public double RateMean { get; set; }

    [Column("rate_sigma")]
    public double RateSigma { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("last_played_at")]
    public DateTime? LastPlayedAt { get; set; }

    [Column("total_matches")]
    public int TotalMatches { get; set; } = 0;

    [Column("total_wins")]
    public int TotalWins { get; set; } = 0;

    [Column("memo")]
    public string Memo { get; set; } = string.Empty;

  }
}
