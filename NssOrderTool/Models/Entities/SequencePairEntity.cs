using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("SequencePairs")]
  [PrimaryKey(nameof(PredecessorId), nameof(SuccessorId))] // 複合主キーの指定(EF Core 7+)
  [Comment("プレイヤーの出現順序ペア（前後関係）の出現頻度統計")] // ← テーブルの論理名・説明
  public class SequencePairEntity : ISoftDelete, ITimestamp
  {
    [Column("predecessor_id")]
    [Comment("前方に位置するプレイヤーのID")] // ← カラムの論理名・説明
    public string PredecessorId { get; set; } = "";

    [Column("successor_id")]
    [Comment("後方に位置するプレイヤーのID")]
    public string SuccessorId { get; set; } = "";

    [Column("frequency")]
    [Comment("この順序ペアが観測された累計頻度（回数）")]
    public int Frequency { get; set; } = 0;

    [Column("is_deleted")]
    [Comment("論理削除フラグ（trueで削除済み）")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    [Comment("レコード作成日時")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [Comment("レコード最終更新日時")]
    public DateTime UpdatedAt { get; set; }
  }
}
