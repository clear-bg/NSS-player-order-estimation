namespace NssOrderTool.Models.Domain
{
  /// <summary>
  /// アプリケーション全体のドメインルール（定数）を管理するクラス
  /// </summary>
  public static class RatingConstants
  {
    // --- レート関連の初期値 ---
    public const double DefaultRateMean = 1500.0;
    public const double DefaultRateSigma = 0.0;

    // --- レート計算式（ScoreBasedRatingCalculator）用の定数 ---
    public const double RatingKFactor = 16.0;   // 変動係数K
    public const int TotalArenaRounds = 14;     // アリーナの全ラウンド数
  }
}
