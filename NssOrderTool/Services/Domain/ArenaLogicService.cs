using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Rating;

namespace NssOrderTool.Services.Domain
{
  public class ArenaLogicService
  {
    private readonly IRatingCalculator _ratingCalculator;
    private readonly PlayerRepository _playerRepository;
    private readonly ArenaRepository _arenaRepository;

    public ArenaLogicService(
      IRatingCalculator ratingCalculator,
      PlayerRepository playerRepository,
      ArenaRepository arenaRepository)
    {
      _ratingCalculator = ratingCalculator;
      _playerRepository = playerRepository;
      _arenaRepository = arenaRepository;
    }

    // ラウンドごとの「青チーム」に所属するスロット配置順（1始まりの提供データを0始まりのインデックスに変換して保持）
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
    /// 指定したラウンド・スロット位置のプレイヤーが「青チーム」かどうかを判定する
    /// </summary>
    /// <param name="roundNumber">ラウンド番号 (1-14)</param>
    /// <param name="slotIndex">スロットのインデックス (0始まり: 0=1番目, 7=8番目)</param>
    /// <returns>true: Blue, false: Orange</returns>
    public virtual bool IsBlueTeam(int roundNumber, int slotIndex)
    {
      if (!BlueTeamDefinitions.ContainsKey(roundNumber)) return false;
      return BlueTeamDefinitions[roundNumber].Contains(slotIndex);
    }

    /// <summary>
    /// 指定ラウンドにおける、指定スロットのプレイヤーのチームIDを返す
    /// </summary>
    /// <returns>1: Blue, 2: Orange</returns>
    public virtual int GetTeamId(int roundNumber, int slotIndex)
    {
      return IsBlueTeam(roundNumber, slotIndex) ? 1 : 2;
    }

    /// <summary>
    /// そのラウンドで、指定スロットのプレイヤーが「勝利したか」を判定する
    /// </summary>
    /// <param name="roundNumber">ラウンド番号</param>
    /// <param name="slotIndex">スロットのインデックス</param>
    /// <param name="winningTeam">そのラウンドの勝利チーム (0:なし, 1:Blue, 2:Orange)</param>
    /// <returns>true: 勝利, false: 敗北または無効</returns>
    public virtual bool IsWinner(int roundNumber, int slotIndex, int winningTeam)
    {
      if (winningTeam == 0) return false;
      int myTeam = GetTeamId(roundNumber, slotIndex);
      return myTeam == winningTeam;
    }

    // セッション結果(各プレイヤーの勝利数)に基づいて、全期間とシーズン別のレート計算とDB更新を行う
    public virtual async Task UpdateRatingsAsync(Dictionary<string, int> playerWinCounts, int seasonId) // ★ 引数に seasonId を追加
    {
      if (playerWinCounts == null || playerWinCounts.Count == 0) return;

      var playerIds = playerWinCounts.Keys.ToList();

      // ==========================================
      // 1. 全期間（All-time）のレート計算と更新
      // ==========================================
      var allTimeData = new Dictionary<string, (RatingData, int)>();
      foreach (var kvp in playerWinCounts)
      {
        var player = await _playerRepository.GetPlayerAsync(kvp.Key);
        var currentRate = (player != null) ? new RatingData(player.RateMean, player.RateSigma) : RatingData.Default;
        allTimeData[kvp.Key] = (currentRate, kvp.Value);
      }

      var newAllTimeRatings = _ratingCalculator.CalculateSession(allTimeData);
      await _playerRepository.UpdatePlayerRatingsAsync(newAllTimeRatings);


      // ==========================================
      // 2. シーズン（Season）のレート計算と更新
      // ==========================================
      // シーズン用の現在のレートをDBから取得
      var seasonRatings = await _playerRepository.GetPlayerSeasonRatingsAsync(playerIds, seasonId);
      var seasonData = new Dictionary<string, (RatingData, int)>();

      foreach (var kvp in playerWinCounts)
      {
        // そのシーズンで初めて試合をする人は、デフォルトレート（1500）からスタート
        var currentSeasonRate = seasonRatings.ContainsKey(kvp.Key) ? seasonRatings[kvp.Key] : RatingData.Default;
        seasonData[kvp.Key] = (currentSeasonRate, kvp.Value);
      }

      var newSeasonRatings = _ratingCalculator.CalculateSession(seasonData);
      // シーズン成績テーブルに保存（試合数や勝数も一緒に渡す）
      await _playerRepository.UpdatePlayerSeasonRatingsAsync(newSeasonRatings, playerWinCounts, seasonId);

      foreach (var kvp in newSeasonRatings)
      {
        var history = new RateHistoryEntity
        {
          PlayerId = kvp.Key,
          Rate = kvp.Value.Mean,
          RecordedAt = DateTime.Now,
          SeasonId = seasonId // マイグレーションで追加したカラム
        };
        await _arenaRepository.AddRateHistoryAsync(history);
      }
    }
  }
}
