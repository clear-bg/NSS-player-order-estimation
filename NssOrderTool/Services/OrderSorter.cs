using System;
using System.Collections.Generic;
using System.Linq;

namespace NssOrderTool.Services
{
    public class OrderSorter
    {
        /// <summary>
        /// 順序ペアのリストから、トポロジカルソートを用いて全体の順序を決定する
        /// </summary>
        public List<string> Sort(List<OrderPair> pairs)
        {
            // 1. グラフの構築 (入次数マップと隣接リスト)
            var inDegree = new Dictionary<string, int>(); // 入ってくるエッジの数
            var adj = new Dictionary<string, List<string>>(); // 出ていくエッジの先
            var nodes = new HashSet<string>();

            // ノードとエッジの登録
            foreach (var pair in pairs)
            {
                nodes.Add(pair.Predecessor);
                nodes.Add(pair.Successor);

                if (!adj.ContainsKey(pair.Predecessor)) adj[pair.Predecessor] = new List<string>();
                if (!adj.ContainsKey(pair.Successor)) adj[pair.Successor] = new List<string>();

                if (!inDegree.ContainsKey(pair.Predecessor)) inDegree[pair.Predecessor] = 0;
                if (!inDegree.ContainsKey(pair.Successor)) inDegree[pair.Successor] = 0;

                // エッジ追加: Predecessor -> Successor
                adj[pair.Predecessor].Add(pair.Successor);
                inDegree[pair.Successor]++;
            }

            // 2. トポロジカルソート (Kahn's Algorithm)
            // 入次数が0（自分より前の要素がない）ノードをキューに入れる
            var queue = new Queue<string>();
            foreach (var node in nodes)
            {
                if (inDegree[node] == 0)
                {
                    queue.Enqueue(node);
                }
            }

            var result = new List<string>();
            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                result.Add(u);

                if (adj.ContainsKey(u))
                {
                    foreach (var v in adj[u])
                    {
                        inDegree[v]--;
                        if (inDegree[v] == 0)
                        {
                            queue.Enqueue(v);
                        }
                    }
                }
            }

            // 閉路検出: ソート結果の数がノード総数と一致しない場合、循環参照（矛盾）が存在する
            if (result.Count != nodes.Count)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ 順序矛盾（閉路）が検出されました。");
                return new List<string>();
            }

            return result;
        }
    }
}