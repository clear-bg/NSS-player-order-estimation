namespace NssOrderTool.Services.Rating
{
  /// <summary>
  /// レーティング情報を持つ不変クラス
  /// </summary>
  public record RatingData(double Mean, double Sigma)
  {
    // 初期レート: 1500
    public static RatingData Default => new(1500.0, 0.0);
  }
}
