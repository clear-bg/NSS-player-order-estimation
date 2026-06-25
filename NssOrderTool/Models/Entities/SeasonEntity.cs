using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NssOrderTool.Models.Entities
{
  [Table("Seasons")]
  public class SeasonEntity
  {
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // --- Navigation Properties ---
    public List<ArenaSessionEntity> ArenaSessions { get; set; } = new();
  }
}
