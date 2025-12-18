using System;
using System.Collections.Generic;
using System.Linq;

namespace NssOrderTool.Services.Domain
{
  public class ArenaLogicService
  {
    // ラウンドごとの「青チーム」に所属するランク順位（1始まりの提供データを0始まりのインデックスに変換して保持）
    // R1: 1, 2, 3, 4 -> {0, 1, 2, 3}
    private static readonly Dictionary<int, int[]> BlueTeamDefinitions = new()
    {
        { 1,  new[] { 0, 1, 2, 3 } }, // R1: 1, 2, 3, 4
        { 2,  new[] { 0, 2, 4, 6 } }, // R2: 1, 3, 5, 7
        { 3,  new[] { 0, 3, 4, 7 } }, // R3: 1, 4, 5, 8
        { 4,  new[] { 0, 1, 6, 7 } }, // R4: 1, 2, 7, 8
        { 5,  new[] { 0, 2, 5, 7 } }, // R5: 1, 3, 6, 8
        { 6,  new[] { 0, 1, 4, 5 } }, // R6: 1, 2, 5, 6
        { 7,  new[] { 0, 3, 5, 6 } }, // R7: 1, 4, 6, 7
        { 8,  new[] { 0, 1, 2, 4 } }, // R8: 1, 2, 3, 5
        { 9,  new[] { 0, 3, 4, 6 } }, // R9: 1, 4, 5, 7
        { 10, new[] { 0, 1, 3, 7 } }, // R10: 1, 2, 4, 8
        { 11, new[] { 0, 2, 3, 5 } }, // R11: 1, 3, 4, 6
        { 12, new[] { 0, 2, 6, 7 } }, // R12: 1, 3, 7, 8
        { 13, new[] { 0, 1, 5, 6 } }, // R13: 1, 2, 6, 7
        { 14, new[] { 0, 4, 5, 7 } }  // R14: 1, 5, 6, 8
    };

    /// <summary>
    /// 指定したラウンド・順位のプレイヤーが「青チーム」かどうかを判定する
    /// </summary>
    /// <param name="roundNumber">ラウンド番号 (1-14)</param>
    /// <param name="rankIndex">順位インデックス (0始まり: 0=1位, 7=8位)</param>
    /// <returns>true: Blue, false: Orange</returns>
    public virtual bool IsBlueTeam(int roundNumber, int rankIndex)
    {
      if (!BlueTeamDefinitions.ContainsKey(roundNumber)) return false;
      return BlueTeamDefinitions[roundNumber].Contains(rankIndex);
    }

    /// <summary>
    /// 指定ラウンドにおける、指定ランクのプレイヤーのチームIDを返す
    /// </summary>
    /// <returns>1: Blue, 2: Orange</returns>
    public virtual int GetTeamId(int roundNumber, int rankIndex)
    {
      return IsBlueTeam(roundNumber, rankIndex) ? 1 : 2;
    }

    /// <summary>
    /// そのラウンドで、指定ランクのプレイヤーが「勝利したか」を判定する
    /// </summary>
    /// <param name="roundNumber">ラウンド番号</param>
    /// <param name="rankIndex">順位インデックス</param>
    /// <param name="winningTeam">そのラウンドの勝利チーム (0:なし, 1:Blue, 2:Orange)</param>
    /// <returns>true: 勝利, false: 敗北または無効</returns>
    public virtual bool IsWinner(int roundNumber, int rankIndex, int winningTeam)
    {
      if (winningTeam == 0) return false;
      int myTeam = GetTeamId(roundNumber, rankIndex);
      return myTeam == winningTeam;
    }
  }
}
