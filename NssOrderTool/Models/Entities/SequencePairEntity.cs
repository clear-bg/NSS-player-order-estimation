using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // 複合キー設定に使用
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("SequencePairs")]
  [PrimaryKey(nameof(PredecessorId), nameof(SuccessorId))] // 複合主キーの指定(EF Core 7+)
  public class SequencePairEntity : ISoftDelete, ITimestamp
  {
    [Column("predecessor_id")]
    public string PredecessorId { get; set; } = "";

    [Column("successor_id")]
    public string SuccessorId { get; set; } = "";

    [Column("frequency")]
    public int Frequency { get; set; } = 0;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
  }
}
