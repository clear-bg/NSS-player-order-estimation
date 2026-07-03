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
  [Comment("アリーナ（対戦環境）の1セッション（試合単位）を管理するテーブル")]
  public class ArenaSessionEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("session_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("セッションのサロゲートキー（自動インクリメントID）")]
    public int Id { get; set; }

    [Column("is_deleted")]
    [Comment("論理削除フラグ（trueで削除済み）")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    [Comment("レコード作成日時")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [Comment("レコード最終更新日時")]
    public DateTime UpdatedAt { get; set; }

    [Column("session_date")]
    [Comment("セッションが実際に実施・観測された日時")]
    public DateTime SessionDate { get; set; }

    [Column("season_id")]
    [Comment("このセッションが属するシーズンのID（Seasonsテーブルの外部キー）")]
    public int SeasonId { get; set; } = 0;

    [Column("is_valid")]
    [Comment("セッションの対戦結果が有効かどうかのフラグ（無効試合の除外用）")]
    public bool IsValid { get; set; } = true;

    [Column("memo")]
    [Comment("セッションに関する自由記述のメモ")]
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
