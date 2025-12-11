using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // 複合キー設定に使用

namespace NssOrderTool.Models.Entities
{
    [Table("SequencePairs")]
    [PrimaryKey(nameof(PredecessorId), nameof(SuccessorId))] // 複合主キーの指定(EF Core 7+)
    public class SequencePairEntity
    {
        [Column("predecessor_id")]
        public string PredecessorId { get; set; } = "";

        [Column("successor_id")]
        public string SuccessorId { get; set; } = "";

        [Column("frequency")]
        public int Frequency { get; set; } = 0;
    }
}