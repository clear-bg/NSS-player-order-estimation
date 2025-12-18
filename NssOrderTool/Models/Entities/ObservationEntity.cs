using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NssOrderTool.Models.Entities
{
  [Table("Observations")]
  public class ObservationEntity
  {
    [Key]
    [Column("observation_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // AUTO_INCREMENT
    public int Id { get; set; }

    [Column("ordered_list")]
    public string OrderedList { get; set; } = "";

    [Column("observation_time")]
    public DateTime ObservationTime { get; set; } = DateTime.Now;
  }
}
