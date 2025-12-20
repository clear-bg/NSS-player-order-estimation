using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("Observations")]
  public class ObservationEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("observation_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // AUTO_INCREMENT
    public int Id { get; set; }

    [Column("observation_time")]
    public DateTime ObservationTime { get; set; } = DateTime.Now;
    public List<ObservationDetailEntity> Details { get; set; } = new();

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
  }
}
