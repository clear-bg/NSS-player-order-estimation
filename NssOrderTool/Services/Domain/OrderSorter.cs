using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // 追加
using Microsoft.Extensions.Logging;
using NssOrderTool.Models;

namespace NssOrderTool.Services.Domain
{
    public class OrderSorter
    {
        private readonly ILogger<OrderSorter> _logger;

        public OrderSorter(ILogger<OrderSorter> logger)
        {
            _logger = logger;
        }

        // ▼▼▼ ここから下を既存の Sort メソッドと置き換えてください ▼▼▼

        /// <summary>
        /// 強連結成分分解(SCC)を用いて、サイクルを含むグラフの順序を推定する
        /// </summary>
        public List<List<string>> Sort(List<OrderPair> pairs)
        {
            // 1. ノードと隣接リストの構築
            var adj = new Dictionary<string, List<string>>();
            var nodes = new HashSet<string>();

            foreach (var pair in pairs)
            {
                nodes.Add(pair.Predecessor);
                nodes.Add(pair.Successor);

                if (!adj.ContainsKey(pair.Predecessor)) adj[pair.Predecessor] = new List<string>();
                if (!adj.ContainsKey(pair.Successor)) adj[pair.Successor] = new List<string>();

                adj[pair.Predecessor].Add(pair.Successor);
            }

            // 2. Tarjanのアルゴリズムで強連結成分(SCC)を抽出
            var sccComponents = TarjanSCC(nodes.ToList(), adj);

            // 3. 成分グラフの構築 (成分を1つのノードとみなす)
            var componentMap = new Dictionary<int, List<string>>();
            var nodeToComponentId = new Dictionary<string, int>();

            for (int i = 0; i < sccComponents.Count; i++)
            {
                componentMap[i] = sccComponents[i];
                foreach (var node in sccComponents[i])
                {
                    nodeToComponentId[node] = i;
                }
            }

            var sccAdj = new Dictionary<int, HashSet<int>>();
            var sccInDegree = new Dictionary<int, int>();

            for (int i = 0; i < sccComponents.Count; i++)
            {
                sccAdj[i] = new HashSet<int>();
                sccInDegree[i] = 0;
            }

            foreach (var u in nodes)
            {
                if (!adj.ContainsKey(u)) continue;
                int uComp = nodeToComponentId[u];

                foreach (var v in adj[u])
                {
                    int vComp = nodeToComponentId[v];
                    if (uComp != vComp)
                    {
                        if (!sccAdj[uComp].Contains(vComp))
                        {
                            sccAdj[uComp].Add(vComp);
                            sccInDegree[vComp]++;
                        }
                    }
                }
            }

            // 4. 成分グラフの階層的トポロジカルソート
            var resultLayers = new List<List<string>>();
            var queue = new Queue<int>();

            foreach (var key in sccInDegree.Keys)
            {
                if (sccInDegree[key] == 0) queue.Enqueue(key);
            }

            while (queue.Count > 0)
            {
                var currentLayerNodes = new List<string>();
                int layerSize = queue.Count;

                for (int i = 0; i < layerSize; i++)
                {
                    var uComp = queue.Dequeue();
                    var members = componentMap[uComp];
                    members.Sort();
                    currentLayerNodes.AddRange(members);

                    if (sccAdj.ContainsKey(uComp))
                    {
                        foreach (var vComp in sccAdj[uComp])
                        {
                            sccInDegree[vComp]--;
                            if (sccInDegree[vComp] == 0) queue.Enqueue(vComp);
                        }
                    }
                }
                if (currentLayerNodes.Count > 0) resultLayers.Add(currentLayerNodes);
            }

            return resultLayers;
        }

        /// <summary>
        /// グラフ内の閉路（矛盾）を1つ検出し、そのパス（例: A -> B -> C -> A）を返す。
        /// </summary>
        public List<string>? FindCyclePath(IEnumerable<OrderPair> pairs)
        {
            var adj = new Dictionary<string, List<string>>();
            foreach (var p in pairs)
            {
                if (!adj.ContainsKey(p.Predecessor)) adj[p.Predecessor] = new List<string>();
                if (!adj.ContainsKey(p.Successor)) adj[p.Successor] = new List<string>();
                adj[p.Predecessor].Add(p.Successor);
            }

            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();
            var pathStack = new List<string>();

            foreach (var node in adj.Keys)
            {
                if (Dfs(node, adj, visited, recursionStack, pathStack))
                {
                    var cycleEndNode = pathStack.Last();
                    var cycleStartIndex = pathStack.IndexOf(cycleEndNode);
                    // サイクル部分のみを抽出して返す
                    return pathStack.Skip(cycleStartIndex).ToList();
                }
            }
            return null;
        }

        private bool Dfs(string current, Dictionary<string, List<string>> adj,
                         HashSet<string> visited, HashSet<string> recursionStack, List<string> pathStack)
        {
            visited.Add(current);
            recursionStack.Add(current);
            pathStack.Add(current);

            if (adj.ContainsKey(current))
            {
                foreach (var neighbor in adj[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        if (Dfs(neighbor, adj, visited, recursionStack, pathStack)) return true;
                    }
                    else if (recursionStack.Contains(neighbor))
                    {
                        pathStack.Add(neighbor); // 閉路完成
                        return true;
                    }
                }
            }

            recursionStack.Remove(current);
            pathStack.RemoveAt(pathStack.Count - 1);
            return false;
        }

        // --- Tarjan's Algorithm (SCC用) ---
        private List<List<string>> TarjanSCC(List<string> nodes, Dictionary<string, List<string>> adj)
        {
            var index = 0;
            var indices = new Dictionary<string, int>();
            var lowLink = new Dictionary<string, int>();
            var onStack = new HashSet<string>();
            var stack = new Stack<string>();
            var result = new List<List<string>>();

            foreach (var node in nodes)
            {
                if (!indices.ContainsKey(node)) StrongConnect(node);
            }

            void StrongConnect(string v)
            {
                indices[v] = index;
                lowLink[v] = index;
                index++;
                stack.Push(v);
                onStack.Add(v);

                if (adj.ContainsKey(v))
                {
                    foreach (var w in adj[v])
                    {
                        if (!indices.ContainsKey(w))
                        {
                            StrongConnect(w);
                            lowLink[v] = Math.Min(lowLink[v], lowLink[w]);
                        }
                        else if (onStack.Contains(w))
                        {
                            lowLink[v] = Math.Min(lowLink[v], indices[w]);
                        }
                    }
                }

                if (lowLink[v] == indices[v])
                {
                    var component = new List<string>();
                    string w;
                    do
                    {
                        w = stack.Pop();
                        onStack.Remove(w);
                        component.Add(w);
                    } while (w != v);
                    result.Add(component);
                }
            }
            return result;
        }
    }
}