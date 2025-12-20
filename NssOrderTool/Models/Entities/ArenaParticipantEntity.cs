using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NssOrderTool.Models.Entities
{
  [Table("ArenaParticipants")]
  public class ArenaParticipantEntity
  {
    [Key]
    [Column("participant_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("session_id")]
    public int SessionId { get; set; }

    // プレイヤーテーブルへの外部キー
    [Column("player_id")]
    public string PlayerId { get; set; } = "";

    // 0-7 の座席番号（画面上の表示順）
    [Column("slot_index")]
    public int SlotIndex { get; set; }

    // そのセッションでの勝利数（スナップショット）
    [Column("win_count")]
    public int WinCount { get; set; }

    // そのセッションでの順位（スナップショット）
    [Column("rank")]
    public int Rank { get; set; }

    // --- Navigation Properties ---

    [ForeignKey(nameof(SessionId))]
    public ArenaSessionEntity? Session { get; set; }

    [ForeignKey(nameof(PlayerId))]
    public PlayerEntity? Player { get; set; }
  }
}
