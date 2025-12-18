using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;
using NssOrderTool.ViewModels;
using Xunit;
using NssOrderTool.Database; // AppDbContext用

namespace NssOrderTool.Tests.ViewModels
{
  public class ArenaViewModelTests
  {
    private readonly Mock<ArenaRepository> _mockRepo;
    private readonly ArenaViewModel _viewModel;

    public ArenaViewModelTests()
    {
      // Repositoryのモック作成 (引数のAppDbContextはnullでOK)
      _mockRepo = new Mock<ArenaRepository>((AppDbContext)null!);

      _viewModel = new ArenaViewModel(_mockRepo.Object);
    }

    [Fact]
    public void Initialize_ShouldCreate14Rounds()
    {
      // Arrange & Act (コンストラクタで初期化済み)

      // Assert
      _viewModel.RoundInputs.Should().HaveCount(14);
      _viewModel.RoundInputs.First().RoundNumber.Should().Be(1);
      _viewModel.RoundInputs.Last().RoundNumber.Should().Be(14);
    }

    [Fact]
    public void ToggleWinner_ShouldRotateStatus()
    {
      // Arrange
      var round1 = _viewModel.RoundInputs.First();
      // 初期状態は 0 (未選択)
      round1.WinningTeam.Should().Be(0);

      // Act & Assert

      // 1回クリック -> 1 (Blue)
      round1.ToggleWinnerCommand.Execute(null);
      round1.WinningTeam.Should().Be(1);
      round1.DisplayText.Should().Be("Blue");

      // 2回クリック -> 2 (Orange)
      round1.ToggleWinnerCommand.Execute(null);
      round1.WinningTeam.Should().Be(2);
      round1.DisplayText.Should().Be("Org");

      // 3回クリック -> 0 (未選択に戻る)
      round1.ToggleWinnerCommand.Execute(null);
      round1.WinningTeam.Should().Be(0);
    }

    [Fact]
    public async Task SaveSession_ShouldCallRepository_AndResetInputs()
    {
      // Arrange
      // 1R: Blue(1), 2R: Orange(2) に設定してみる
      _viewModel.RoundInputs[0].WinningTeam = 1;
      _viewModel.RoundInputs[1].WinningTeam = 2;

      // Act
      await _viewModel.SaveSessionCommand.ExecuteAsync(null);

      // Assert
      // 1. リポジトリのAddSessionAsyncが呼ばれたか検証
      _mockRepo.Verify(r => r.AddSessionAsync(It.Is<ArenaSessionEntity>(s =>
        s.Rounds.Count == 14 &&
        s.Rounds.Any(r => r.RoundNumber == 1 && r.WinningTeam == 1) &&
        s.Rounds.Any(r => r.RoundNumber == 2 && r.WinningTeam == 2)
      )), Times.Once);

      // 2. 入力がリセットされたか検証 (全て0に戻っているはず)
      _viewModel.RoundInputs.All(r => r.WinningTeam == 0).Should().BeTrue();

      // 3. ステータステキストが成功メッセージになっているか
      _viewModel.StatusText.Should().Contain("保存しました");
    }
  }
}
