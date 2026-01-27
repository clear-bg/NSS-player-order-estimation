using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NssOrderTool.Models.Entities
{
  [Table("RateHistories")]
  public class RateHistoryEntity
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public string PlayerId { get; set; } = "";

    [Required]
    public double Rate { get; set; }

    [Required]
    public DateTime RecordedAt { get; set; }
  }
}
