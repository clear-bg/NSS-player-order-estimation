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
    public List<OrderPair> ExtractPairs(List<string> playerIds)
    {
      var relationships = new List<OrderPair>();

      if (playerIds == null || playerIds.Count < 2)
        return relationships;

      // 前後の要素同士で「隣接ペア」を生成する（A->B, B->C）
      for (int i = 0; i < playerIds.Count - 1; i++)
      {
        relationships.Add(new OrderPair(playerIds[i], playerIds[i + 1]));
      }

      return relationships;
    }
  }
}
