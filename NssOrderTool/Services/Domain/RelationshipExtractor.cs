using System.Collections.Generic;
using System.Linq;
using NssOrderTool.Models;

namespace NssOrderTool.Services.Domain
{
  // 順序関係を表すレコード
  // Predecessor: 順序が先のプレイヤー
  // Successor: 順序が後のプレイヤー

  public class RelationshipExtractor
  {
    public string NormalizeInput(string inputLine, Dictionary<string, string> aliasDict)
    {
      if (string.IsNullOrWhiteSpace(inputLine)) return "";

      // 1. 分解
      var rawNames = inputLine.Split(',')
                              .Select(p => p.Trim())
                              .Where(p => !string.IsNullOrEmpty(p))
                              .ToList();

      // 2. 変換
      var convertedNames = new List<string>();
      foreach (var name in rawNames)
      {
        // 辞書にあれば変換、なければそのまま
        if (aliasDict.TryGetValue(name, out string? target))
        {
          convertedNames.Add(target);
        }
        else
        {
          convertedNames.Add(name);
        }
      }

      // 3. 再結合
      return string.Join(", ", convertedNames);
    }

    /// <summary>
    /// カンマ区切りのプレイヤーリストから、順序ペアを抽出する
    /// 例: "A, B, C" -> (A,B), (A,C), (B,C)
    /// </summary>
    public List<OrderPair> ExtractFromInput(string inputLine)
    {
      if (string.IsNullOrWhiteSpace(inputLine))
        return new List<OrderPair>();

      // カンマ区切りで分割し、余計な空白を除去
      var players = inputLine.Split(',')
                             .Select(p => p.Trim())
                             .Where(p => !string.IsNullOrEmpty(p))
                             .ToList();

      var relationships = new List<OrderPair>();

      // 前方の要素は、後方の全要素に対して「先である」という関係を持つ
      for (int i = 0; i < players.Count; i++)
      {
        for (int j = i + 1; j < players.Count; j++)
        {
          relationships.Add(new OrderPair(players[i], players[j]));
        }
      }

      return relationships;
    }
  }
}