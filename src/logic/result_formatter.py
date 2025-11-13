import networkx as nx
from typing import List, Union, Set

def format_ranking(sorted_list: List[str], G: nx.DiGraph) -> List[Union[str, Set[str]]]:
    """
    トポロジカルソート結果とグラフを基に、順序が未確定な隣接ノードを同率グループとしてまとめる。

    Args:
        sorted_list: トポロジカルソートの結果 (ノードのリスト)。
        G: 順序関係を示す有向グラフ (networkx.DiGraph)。

    Returns:
        プレイヤーID（確定）またはプレイヤーIDのセット（同率グループ）のリスト。
    """
    if not sorted_list:
        return []

    ranked_groups: List[Union[str, Set[str]]] = []
    current_group: Set[str] = {sorted_list[0]}

    # ソートされたリストを前から順にチェック
    for i in range(1, len(sorted_list)):
        current_node = sorted_list[i-1]
        next_node = sorted_list[i]

        # 同率と見なす条件: 互いの間に順序関係のエッジがない (どちらの方向にもエッジがない)
        has_forward_edge = G.has_edge(current_node, next_node)
        has_backward_edge = G.has_edge(next_node, current_node)

        if not has_forward_edge and not has_backward_edge:
            # エッジが存在しないため、前ノードと同率グループに入れる
            current_group.add(next_node)
        else:
            # エッジが存在する (順序が確定している)

            # 確定したグループを ranked_groups に移動
            if len(current_group) == 1:
                ranked_groups.append(current_group.pop()) # 単一要素なら文字列として追加
            else:
                ranked_groups.append(current_group) # 複数要素ならセットとして追加

            # 新しいグループを開始
            current_group = {next_node}

    # 最後のグループを処理
    if len(current_group) == 1:
        ranked_groups.append(current_group.pop())
    else:
        ranked_groups.append(current_group)

    return ranked_groups

def assign_rank_and_print(ranked_groups: List[Union[str, Set[str]]]):
    """
    整形された順序グループに順位を割り当てて表示する。
    """
    print("\n--- 確定/推定されたプレイヤー順位 ---")
    print("---------------------------------------")
    current_rank = 1
    for group in ranked_groups:
        if isinstance(group, str):
            print(f"[{current_rank:>2}] {group}")
            current_rank += 1
        elif isinstance(group, set):
            players_str = ", ".join(sorted(list(group)))
            print(f"[{current_rank:>2}] {players_str} (同率グループ)")
            # グループ全体で一つの順位を占める
            current_rank += 1
    print("---------------------------------------")
    return
