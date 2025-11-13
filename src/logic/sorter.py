# order_tool/logic/sorter.py

import networkx as nx
from typing import List, Tuple, Union

# エッジデータの型定義: (superior_id, inferior_id, frequency)
RelationshipData = List[Tuple[str, str, int]]

def build_weighted_graph(relationships: RelationshipData) -> nx.DiGraph:
    """
    優劣関係データから重み付き有向グラフを構築する。
    頻度(frequency)をエッジの重みとする。
    """
    G = nx.DiGraph()
    for superior, inferior, frequency in relationships:
        # 重複するエッジの重みはnetworkxが自動で加算・更新する
        # ここでは、DBから取得したfrequencyが既に集計済みであることを前提とする
        G.add_edge(superior, inferior, weight=frequency)
    return G

def find_player_order(G: nx.DiGraph) -> Union[List[str], Tuple[str, str]]:
    """
    グラフからプレイヤーの順序を推定する (トポロジカルソート)。
    矛盾がある場合は、そのサイクルを報告する。
    """
    try:
        # トポロジカルソートを実行
        sorted_nodes = list(nx.topological_sort(G))
        return sorted_nodes
    except nx.NetworkXUnfeasible:
        # グラフにサイクル（矛盾）がある場合

        # 矛盾を引き起こしているサイクルを検出
        try:
            # networkx.find_cycle は、見つかった最初の一つを返す
            # 戻り値の形式: [(u1, v1, key1), (v1, v2, key2), ...]
            cycle = nx.find_cycle(G, orientation='original')

            # サイクルに含まれるノードを抽出して報告
            # サイクル内のノードをセットにして重複を排除
            nodes_in_cycle = set()
            for u, v, data in cycle:
                nodes_in_cycle.add(u)
                nodes_in_cycle.add(v)

            # 報告しやすい形式に整形して返す
            return ("Cycle Detected", ", ".join(sorted(list(nodes_in_cycle))))

        except nx.NetworkXNoCycle:
            # 理論上ありえないが、念のため
            return ("Cycle Detected", "A contradiction exists, but cycle could not be identified.")

def analyze_and_rank(conn) -> Union[List[str], Tuple[str, str]]:
    """
    DBからデータを取得し、グラフを構築・分析して順序を推定するメイン関数。
    """
    # 依存関係を外部からインポートする必要がある
    from src.core.db_manager import fetch_all_relationships

    # 1. DBから優劣関係データを取得
    relationships = fetch_all_relationships(conn)

    if not relationships:
        return ["No relationship data found."]

    # 2. グラフを構築
    G = build_weighted_graph(relationships)

    # 3. 順序の推定
    result = find_player_order(G)

    return result

if __name__ == '__main__':
    # このテストはDB接続が必要なため、main.pyから実行するのが望ましい
    print("--- sorter.py の単体テストは main.py から実行してください ---")