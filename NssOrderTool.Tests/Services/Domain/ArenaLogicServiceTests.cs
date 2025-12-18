using FluentAssertions;
using NssOrderTool.Services.Domain;
using Xunit;

namespace NssOrderTool.Tests.Services.Domain
{
  public class ArenaLogicServiceTests
  {
    private readonly ArenaLogicService _service;

    public ArenaLogicServiceTests()
    {
      _service = new ArenaLogicService();
    }

    [Theory]
    // --- R1: 1, 2, 3, 4 が Blue (Index 0,1,2,3) ---
    [InlineData(1, 0, true)]  // Rank 1 -> Blue
    [InlineData(1, 3, true)]  // Rank 4 -> Blue
    [InlineData(1, 4, false)] // Rank 5 -> Orange
    [InlineData(1, 7, false)] // Rank 8 -> Orange

    // --- R2: 1, 3, 5, 7 が Blue (Index 0,2,4,6) ---
    [InlineData(2, 0, true)]  // Rank 1 -> Blue
    [InlineData(2, 1, false)] // Rank 2 -> Orange
    [InlineData(2, 2, true)]  // Rank 3 -> Blue
    [InlineData(2, 6, true)]  // Rank 7 -> Blue

    // --- R14: 1, 5, 6, 8 が Blue (Index 0,4,5,7) ---
    [InlineData(14, 0, true)]  // Rank 1 -> Blue
    [InlineData(14, 4, true)]  // Rank 5 -> Blue
    [InlineData(14, 1, false)] // Rank 2 -> Orange
    public void IsBlueTeam_ShouldReturnCorrectTeam_AccordingToDefinition(int round, int rankIndex, bool expectedBlue)
    {
      // Act
      var result = _service.IsBlueTeam(round, rankIndex);

      // Assert
      result.Should().Be(expectedBlue, $"Round {round}, RankIndex {rankIndex} should be {(expectedBlue ? "Blue" : "Orange")}");
    }

    [Theory]
    // Round 1 (Rank1 is Blue, Rank5 is Orange)
    // 勝利チームID: 0=None, 1=Blue, 2=Orange

    // 自分がBlue、Blueが勝ち -> 勝ち
    [InlineData(1, 0, 1, true)]
    // 自分がBlue、Orangeが勝ち -> 負け
    [InlineData(1, 0, 2, false)]

    // 自分がOrange、Orangeが勝ち -> 勝ち
    [InlineData(1, 4, 2, true)]
    // 自分がOrange、Blueが勝ち -> 負け
    [InlineData(1, 4, 1, false)]

    // 引き分け/無効 -> 負け扱い(false)
    [InlineData(1, 0, 0, false)]
    public void IsWinner_ShouldReturnTrue_WhenMyTeamWins(int round, int rankIndex, int winningTeam, bool expectedWin)
    {
      // Act
      var result = _service.IsWinner(round, rankIndex, winningTeam);

      // Assert
      result.Should().Be(expectedWin);
    }
  }
}
