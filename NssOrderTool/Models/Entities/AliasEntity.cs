using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("Aliases")]
  [Comment("プレイヤーの別名（エイリアス）を管理するテーブル")]
  public class AliasEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("id")]
    [Comment("サロゲートキー（自動インクリメントID）")]
    public int Id { get; set; }

    [Column("alias_name")]
    [Comment("観測時に入力されたプレイヤーの別名")]
    public string AliasName { get; set; } = "";

    [Column("target_player_id")]
    [Comment("紐付け先となる正規のプレイヤーID（Playersテーブルの外部キー）")]
    public string TargetPlayerId { get; set; } = "";

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
