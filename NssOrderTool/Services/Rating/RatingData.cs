namespace NssOrderTool.Services.Rating
{
  /// <summary>
  /// レーティング情報（強さと不確実性）を持つ不変クラス
  /// </summary>
  public class RatingData
  {
    public double Mean { get; }
    public double Sigma { get; }

    public RatingData(double mean, double sigma)
    {
      Mean = mean;
      Sigma = sigma;
    }

    /// デフォルトの初期値 (OpenSkill標準: Mean=25, Sigma=25/3)
    public static RatingData Default => new RatingData(25.0, 25.0 / 3.0);
  }
}
