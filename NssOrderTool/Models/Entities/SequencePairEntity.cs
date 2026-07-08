using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("SequencePairs")]
  [PrimaryKey(nameof(PredecessorId), nameof(SuccessorId))] // 複合主キーの指定(EF Core 7+)
  [Comment("出現順序ペアの頻度統計テーブル")] // ← テーブルの論理名・説明
  public class SequencePairEntity : ISoftDelete, ITimestamp
  {
    [Column("predecessor_id")]
    [Comment("前方プレイヤーID")] // ← カラムの論理名・説明
    public string PredecessorId { get; set; } = "";

    [Column("successor_id")]
    [Comment("後方プレイヤーID")]
    public string SuccessorId { get; set; } = "";

    [Column("frequency")]
    [Comment("累計出現頻度")]
    public int Frequency { get; set; } = 0;

    [Column("is_deleted")]
    [Comment("論理削除フラグ")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    [Comment("作成日時")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [Comment("更新日時")]
    public DateTime UpdatedAt { get; set; }
  }
}
