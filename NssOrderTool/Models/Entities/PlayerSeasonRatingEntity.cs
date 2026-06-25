using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("PlayerSeasonRatings")]
  public class PlayerSeasonRatingEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("player_id")]
    public string PlayerId { get; set; } = string.Empty;

    [Column("season_id")]
    public int SeasonId { get; set; }

    [Column("rate_mean")]
    public double RateMean { get; set; }

    [Column("rate_sigma")]
    public double RateSigma { get; set; }

    [Column("total_matches")]
    public int TotalMatches { get; set; } = 0;

    [Column("total_wins")]
    public int TotalWins { get; set; } = 0;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // --- Navigation Properties ---
    [ForeignKey(nameof(PlayerId))]
    public PlayerEntity? Player { get; set; }

    [ForeignKey(nameof(SeasonId))]
    public SeasonEntity? Season { get; set; }
  }
}
