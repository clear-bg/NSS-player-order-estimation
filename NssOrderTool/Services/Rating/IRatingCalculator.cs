using System.Collections.Generic;

namespace NssOrderTool.Services.Rating
{
  public interface IRatingCalculator
  {
    /// セッション（全試合）終了後のレート計算を行う
    /// <param name="participants">
    /// 参加者リスト。
    /// Key: プレイヤーID
    /// Value: (現在のレートデータ, 14戦中の勝利数)
    /// </param>
    /// <returns>
    /// 計算後の新しいレートデータ
    /// Key: プレイヤーID
    /// Value: 新しいレート
    /// </returns>
    Dictionary<string, RatingData> CalculateSession(
        Dictionary<string, (RatingData CurrentRate, int WinCount)> participants);
  }
}
