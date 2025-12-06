using System;
using System.Collections.Generic;
using System.Linq;
using NssOrderTool.Models;

namespace NssOrderTool.Services
{
    public class OrderSorter
    {
        /// <summary>
        /// 順序ペアのリストから、階層構造（同率含む）を持つランキングを計算する
        /// 戻り値: 1位グループ, 2位グループ... のリスト
        /// </summary>
        public List<List<string>> Sort(List<OrderPair> pairs)
        {
            var inDegree = new Dictionary<string, int>();
            var adj = new Dictionary<string, List<string>>();
            var nodes = new HashSet<string>();

            // 1. グラフ構築
            foreach (var pair in pairs)
            {
                nodes.Add(pair.Predecessor);
                nodes.Add(pair.Successor);

                if (!adj.ContainsKey(pair.Predecessor)) adj[pair.Predecessor] = new List<string>();
                if (!adj.ContainsKey(pair.Successor)) adj[pair.Successor] = new List<string>();

                if (!inDegree.ContainsKey(pair.Predecessor)) inDegree[pair.Predecessor] = 0;
                if (!inDegree.ContainsKey(pair.Successor)) inDegree[pair.Successor] = 0;

                adj[pair.Predecessor].Add(pair.Successor);
                inDegree[pair.Successor]++;
            }

            // 2. 階層的トポロジカルソート
            // 入次数0のノードを全てキューに入れる
            var queue = new Queue<string>();
            foreach (var node in nodes)
            {
                if (!inDegree.ContainsKey(node) || inDegree[node] == 0)
                {
                    queue.Enqueue(node);
                }
            }

            var resultLayers = new List<List<string>>();
            int processedCount = 0;

            while (queue.Count > 0)
            {
                // 現在のキューに入っているノードは、すべて「現時点で前提条件がない」ノードたち。
                // つまり、これらは互いに順序がつかない「同率グループ」とみなせる。
                var currentLayer = new List<string>();
                int layerSize = queue.Count;

                // 現在のレイヤー分だけループを回して取り出す
                for (int i = 0; i < layerSize; i++)
                {
                    var u = queue.Dequeue();
                    currentLayer.Add(u);
                    processedCount++;

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

                // 名前順などで整列しておくと見やすい
                currentLayer.Sort();
                resultLayers.Add(currentLayer);
            }

            // 閉路検出
            if (processedCount != nodes.Count)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ 順序矛盾（閉路）が検出されました。");
                return new List<List<string>>();
            }

            return resultLayers;
        }
    }
}