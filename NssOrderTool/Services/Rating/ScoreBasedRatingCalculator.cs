using System;
using System.Collections.Generic;
using System.Linq;

namespace NssOrderTool.Services.Rating
{
  public class ScoreBasedRatingCalculator : IRatingCalculator
  {
    // 変動係数 K (今回は固定で16)
    private const double K = 16.0;

    // 全試合数 (期待値計算の分母)
    private const int TotalRounds = 14;

    public Dictionary<string, RatingData> CalculateSession(
        Dictionary<string, (RatingData CurrentRate, int WinCount)> participants)
    {
      var results = new Dictionary<string, RatingData>();
      var allPlayerIds = participants.Keys.ToList();

      // 参加者が1人しかいない場合は計算できないのでそのまま返す
      if (allPlayerIds.Count <= 1)
      {
        foreach (var p in participants)
        {
          results[p.Key] = p.Value.CurrentRate;
        }
        return results;
      }

      // 全員分の計算ループ
      foreach (var playerId in allPlayerIds)
      {
        var (myRateData, myWinCount) = participants[playerId];
        double myRate = myRateData.Mean;

        // 1. 自分以外の対戦相手のレート平均を計算
        // (自分以外のレートの合計) / (人数 - 1)
        double otherRatesSum = 0;
        int otherCount = 0;

        foreach (var otherId in allPlayerIds)
        {
          if (otherId == playerId) continue;

          otherRatesSum += participants[otherId].CurrentRate.Mean;
          otherCount++;
        }

        double opponentAvgRate = otherRatesSum / otherCount;

        // 2. 期待勝数 (Expected Score) の計算
        // 勝率 = 1 / (1 + 10^((相手平均 - 自分) / 400))
        // 期待勝数 = 試合数 * 勝率
        double winProbability = 1.0 / (1.0 + Math.Pow(10.0, (opponentAvgRate - myRate) / 400.0));
        double expectedWins = TotalRounds * winProbability;

        // 3. レート変動の計算
        // 変動 = K * (実際の勝数 - 期待勝数)
        double delta = K * (myWinCount - expectedWins);
        double newRateValue = myRate + delta;

        // ※レートがマイナスにならないようにガード（任意）
        if (newRateValue < 0) newRateValue = 0;

        // 結果を格納 (Sigmaは0で固定)
        results[playerId] = new RatingData(newRateValue, 0.0);
      }

      return results;
    }
  }
}
