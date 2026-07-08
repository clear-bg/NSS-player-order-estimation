using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("Aliases")]
  [Comment("プレイヤー別名を管理するテーブル")]
  public class AliasEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("id")]
    [Comment("自動採番ID")]
    public int Id { get; set; }

    [Column("alias_name")]
    [Comment("観測時に入力された別名")]
    public string AliasName { get; set; } = "";

    [Column("target_player_id")]
    [Comment("紐付け先プレイヤーID")]
    public string TargetPlayerId { get; set; } = "";

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
