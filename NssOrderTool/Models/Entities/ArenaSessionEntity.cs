using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("ArenaSessions")]
  [Comment("アリーナ対戦セッションを管理するテーブル")]
  public class ArenaSessionEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("session_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("自動採番ID")]
    public int Id { get; set; }

    [Column("is_deleted")]
    [Comment("論理削除フラグ")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    [Comment("作成日時")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [Comment("更新日時")]
    public DateTime UpdatedAt { get; set; }

    [Column("session_date")]
    [Comment("実施日時")]
    public DateTime SessionDate { get; set; }

    [Column("season_id")]
    [Comment("所属シーズンID")]
    public int SeasonId { get; set; } = 0;

    [Column("is_valid")]
    [Comment("結果有効フラグ")]
    public bool IsValid { get; set; } = true;

    [Column("memo")]
    [Comment("自由記述メモ")]
    public string Memo { get; set; } = string.Empty;

    // --- Navigation Properties ---

    // 参加者リスト
    public List<ArenaParticipantEntity> Participants { get; set; } = new();

    // リレーション (1対多)
    public List<ArenaRoundEntity> Rounds { get; set; } = new();

    [ForeignKey(nameof(SeasonId))]
    public SeasonEntity? Season { get; set; }
  }
}
