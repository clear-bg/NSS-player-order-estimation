using System.Collections.Generic;

namespace NssOrderTool.Models
{
  // リスト表示用のデータクラス
  public class AliasGroupItem
  {
    public string TargetName { get; set; } = "";
    public List<string> Aliases { get; set; } = new();

    // 画面表示用
    public string DisplayText => $"{TargetName} : {string.Join(", ", Aliases)}";
  }
}
