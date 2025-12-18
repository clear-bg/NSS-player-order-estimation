using System.Collections.Generic;
using System.Linq;
using System.Text;
using NssOrderTool.Models;

namespace NssOrderTool.Services.Domain
{
  public class GraphVizService
  {
    public string GenerateMermaid(List<OrderPair> pairs, List<List<string>> sortedLayers)
    {
      var sb = new StringBuilder();

      // ヘッダー: 上から下(TD)へのグラフ
      sb.AppendLine("graph TD;");

      // スタイル定義 (オプション: ノードを見やすくする)
      sb.AppendLine("  classDef default fill:#f9f9f9,stroke:#333,stroke-width:1px;");

      // 1. 同順位（ランク）のグループ化
      // Mermaidの subgraph を使うと、同じボックス内に表示できる
      for (int i = 0; i < sortedLayers.Count; i++)
      {
        var layer = sortedLayers[i];
        if (layer.Count > 0)
        {
          sb.AppendLine($"  subgraph Rank{i + 1} [Rank {i + 1}]");
          sb.AppendLine("    direction LR;"); // ランク内は左から右へ
          foreach (var node in layer)
          {
            // ノード名に特殊文字が含まれる場合の対策（IDとラベルを分けるのが安全だが、今回は簡易的にそのまま）
            sb.AppendLine($"    {Sanitize(node)};");
          }
          sb.AppendLine("  end");
        }
      }

      // 2. エッジ（矢印）の定義
      foreach (var pair in pairs)
      {
        sb.AppendLine($"  {Sanitize(pair.Predecessor)} --> {Sanitize(pair.Successor)};");
      }

      return sb.ToString();
    }

    // Mermaidでエラーになりそうな文字を除去/置換
    private string Sanitize(string name)
    {
      // 空白や記号を含む場合は引用符で囲むなどの処理が必要だが、
      // 現状は簡易的にスペースを除去するか、IDとして安全な文字に変換する
      // ここではシンプルにそのまま返す（必要に応じて拡張）
      return name.Replace(" ", "_");
    }
  }
}
