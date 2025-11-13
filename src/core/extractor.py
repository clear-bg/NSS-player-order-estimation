# order_tool/core/extractor.py

from typing import List, Tuple

def extract_relationships(ordered_list_str: str, observation_id: int) -> List[Tuple[str, str, int, int]]:
    """
    カンマ区切りの順序リストから、すべての優劣関係のペアを抽出する。

    例: 'P3,P1,P4' -> (P3, P1), (P3, P4), (P1, P4)
    Args:
        ordered_list_str: 観測されたプレイヤーIDのカンマ区切り文字列。
        observation_id: この観測に紐づけるID。

    Returns:
        (superior_id, inferior_id, observation_id, frequency=1) のタプルリスト。
    """
    players_in_order = [p.strip() for p in ordered_list_str.split(',') if p.strip()]

    relationship_list = []
    num_players = len(players_in_order)

    # ネストされたループで、前にあるすべてのプレイヤーと後ろにあるすべてのプレイヤーのペアを作る
    for i in range(num_players):
        superior = players_in_order[i]
        for j in range(i + 1, num_players):
            inferior = players_in_order[j]
            # (優位プレイヤーID, 劣位プレイヤーID, 観測ID, 頻度)
            relationship_list.append((superior, inferior, observation_id, 1))

    return relationship_list


if __name__ == '__main__':
    # テスト実行
    test_list = "Player3, Player4, Player8, Player1"
    obs_id = 999

    relationships = extract_relationships(test_list, obs_id)

    print(f"--- 抽出テスト ({test_list}) ---")
    for r in relationships:
        print(f"優位: {r[0]:<8} | 劣位: {r[1]:<8} | 観測ID: {r[2]} | 頻度: {r[3]}")