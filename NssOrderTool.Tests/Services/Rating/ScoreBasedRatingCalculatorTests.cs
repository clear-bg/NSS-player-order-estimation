using System.Collections.Generic;
using FluentAssertions;
using NssOrderTool.Services.Rating;
using Xunit;

namespace NssOrderTool.Tests.Services.Rating
{
  public class ScoreBasedRatingCalculatorTests
  {
    private readonly ScoreBasedRatingCalculator _calculator;

    public ScoreBasedRatingCalculatorTests()
    {
      _calculator = new ScoreBasedRatingCalculator();
    }

    [Fact]
    public void CalculateSession_All1500_7Wins_ShouldNotChangeRating()
    {
      // Arrange
      // 全員レート1500、全員7勝（引き分け状態）のケース
      var participants = new Dictionary<string, (RatingData, int)>();
      for (int i = 0; i < 8; i++)
      {
        participants.Add($"P{i}", (new RatingData(1500, 0), 7));
      }

      // Act
      var results = _calculator.CalculateSession(participants);

      // Assert
      // 期待勝数(7.0) - 実勝数(7.0) = 0 なので変動なし
      foreach (var res in results)
      {
        res.Value.Mean.Should().Be(1500);
      }
    }

    [Fact]
    public void CalculateSession_All1500_14Wins_ShouldIncreaseBy224()
    {
      // Arrange
      // P0が14勝(全勝)、他7人が6勝ずつ(負け越し)
      var participants = new Dictionary<string, (RatingData, int)>();
      participants.Add("Winner", (new RatingData(1500, 0), 14));

      for (int i = 1; i < 8; i++)
      {
        participants.Add($"Loser{i}", (new RatingData(1500, 0), 6));
      }

      // Act
      var results = _calculator.CalculateSession(participants);

      // Assert
      // Winner: 32 * (14 - 7) = +224
      results["Winner"].Mean.Should().Be(1724);

      // Losers: 32 * (6 - 7) = -32
      results["Loser1"].Mean.Should().Be(1468);
    }

    [Fact]
    public void CalculateSession_HighRatePlayer_NeedsMoreWins()
    {
      // Arrange
      // Strong: 1900 (格上), Others: 1500
      // レート差400 -> 勝率約91% -> 期待勝数 約12.7勝
      // なので、12勝してもレートは下がるはず

      var participants = new Dictionary<string, (RatingData, int)>();
      participants.Add("Strong", (new RatingData(1900, 0), 12)); // 12勝

      for (int i = 1; i < 8; i++)
      {
        participants.Add($"Normal{i}", (new RatingData(1500, 0), 2)); // 適当
      }

      // Act
      var results = _calculator.CalculateSession(participants);

      // Assert
      // 期待値(12.7) > 実績(12) なので下がるはず
      results["Strong"].Mean.Should().BeLessThan(1900);
    }
  }
}
