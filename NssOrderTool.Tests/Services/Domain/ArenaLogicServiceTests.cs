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
    // Round 1: 1, 2, 3, 4位が Blue (Index: 0, 1, 2, 3)
    [InlineData(1, 0, true)]  // 1位 -> Blue
    [InlineData(1, 3, true)]  // 4位 -> Blue
    [InlineData(1, 4, false)] // 5位 -> Orange
    [InlineData(1, 7, false)] // 8位 -> Orange
    // Round 2: 1, 3, 5, 7位が Blue (Index: 0, 2, 4, 6)
    [InlineData(2, 0, true)]  // 1位 -> Blue
    [InlineData(2, 1, false)] // 2位 -> Orange
    [InlineData(2, 2, true)]  // 3位 -> Blue
    public void IsBlueTeam_ShouldReturnCorrectTeam_BasedOnDefinitions(int round, int rankIndex, bool expectedBlue)
    {
      // Act
      var result = _service.IsBlueTeam(round, rankIndex);

      // Assert
      result.Should().Be(expectedBlue);
    }

    [Theory]
    [InlineData(1, 0, 1)] // R1, 1位(Blue), Blue=1 -> Blue
    [InlineData(1, 4, 2)] // R1, 5位(Orange), Orange=2 -> Orange
    public void GetTeamId_ShouldReturn1ForBlue_And2ForOrange(int round, int rankIndex, int expectedTeamId)
    {
      // Act
      var result = _service.GetTeamId(round, rankIndex);

      // Assert
      result.Should().Be(expectedTeamId);
    }

    [Theory]
    // R1: 1位(Blue)
    [InlineData(1, 0, 1, true)]  // Blue勝ち -> 勝ち
    [InlineData(1, 0, 2, false)] // Orange勝ち -> 負け
    [InlineData(1, 0, 0, false)] // 引き分け -> 負け(勝利ではない)
    // R1: 5位(Orange)
    [InlineData(1, 4, 2, true)]  // Orange勝ち -> 勝ち
    [InlineData(1, 4, 1, false)] // Blue勝ち -> 負け
    public void IsWinner_ShouldReturnTrue_OnlyWhenMyTeamWins(int round, int rankIndex, int winningTeam, bool expectedWin)
    {
      // Act
      var result = _service.IsWinner(round, rankIndex, winningTeam);

      // Assert
      result.Should().Be(expectedWin);
    }
  }
}
