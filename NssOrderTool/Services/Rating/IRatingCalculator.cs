using System.Collections.Generic;

namespace NssOrderTool.Services.Rating
{
  public interface IRatingCalculator
  {
    /// <summary>
    /// 試合結果に基づいて新しいレートを計算する
    /// </summary>
    /// <param name="team1">チーム1のプレイヤーIDと現在のレート</param>
    /// <param name="team2">チーム2のプレイヤーIDと現在のレート</param>
    /// <param name="winnerTeam">勝者チーム番号 (1: Team1, 2: Team2, 0: 引き分け)</param>
    /// <returns>更新されたプレイヤーIDと新レートの辞書</returns>
    Dictionary<string, RatingData> CalculateMatch(
        IEnumerable<(string PlayerId, RatingData CurrentRating)> team1,
        IEnumerable<(string PlayerId, RatingData CurrentRating)> team2,
        int winnerTeam);
  }
}
