using System;
using NssOrderTool.Models.Entities;

namespace NssOrderTool.Models.UI
{
  public class SeasonUIItem
  {
    // データベースのエンティティ本体を保持
    public SeasonEntity Entity { get; }

    public SeasonUIItem(SeasonEntity entity)
    {
      Entity = entity;
    }

    // 画面バインディング用のプロパティ（Entityから直接取得）
    public string Name => Entity.Name;
    public DateTime StartDate => Entity.StartDate;
    public bool IsActive => Entity.IsActive;

    // ★ 表示用ロジック 1: 終了日が未設定の場合はハイフンを返す
    public string EndDateDisplay => Entity.EndDate.HasValue ? Entity.EndDate.Value.ToString("yyyy/MM/dd") : "-";

    // ★ 表示用ロジック 2: アクティブ状態に応じたアイコンを返す
    public string ActiveStatusIcon => Entity.IsActive ? "🟢" : "🟠";

    public bool CanDelete { get; set; }
  }
}
