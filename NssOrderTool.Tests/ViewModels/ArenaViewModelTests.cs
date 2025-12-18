using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NssOrderTool.Database;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain; // 追加
using NssOrderTool.ViewModels;
using NssOrderTool.ViewModels.Arena; // 追加
using Xunit;

namespace NssOrderTool.Tests.ViewModels
{
  public class ArenaViewModelTests
  {
    private readonly Mock<ArenaRepository> _mockRepo;
    private readonly ArenaLogicService _logicService; // 追加
    private readonly ArenaViewModel _viewModel;

    public ArenaViewModelTests()
    {
      // Repositoryのモック (AppDbContextは使わないのでnull)
      _mockRepo = new Mock<ArenaRepository>((AppDbContext)null!);

      // LogicServiceは本物を使用（依存がないため）
      _logicService = new ArenaLogicService();

      // ViewModel生成 (LogicServiceも渡す)
      _viewModel = new ArenaViewModel(_mockRepo.Object, _logicService);
    }

    [Fact]
    public void Initialize_ShouldCreate14Rounds_And8Players()
    {
      // Assert
      _viewModel.RoundInputs.Should().HaveCount(14);
      _viewModel.RoundInputs.First().RoundNumber.Should().Be(1);

      _viewModel.PlayerRows.Should().HaveCount(8);
      _viewModel.PlayerRows.First().Name.Should().Be("A");
      _viewModel.PlayerRows.Last().Name.Should().Be("H");
    }

    [Fact]
    public void ToggleWinner_ShouldUpdateRowsAndRanks()
    {
      // Arrange
      // R1: 1,2,3,4(Blue) vs 5,6,7,8(Orange)
      // Aさん(Index0)はBlueチーム
      var round1 = _viewModel.RoundInputs.First(r => r.RoundNumber == 1);
      var rowA = _viewModel.PlayerRows.First(p => p.Name == "A");
      var rowH = _viewModel.PlayerRows.First(p => p.Name == "H");

      // Act 1: Blue Win にする
      round1.WinningTeam = 1; // Blue

      // Assert 1
      // AさんはBlueなので勝っているはず
      rowA.Cells[0].IsWinner.Should().BeTrue();
      rowA.Cells[0].ResultMark.Should().Be("Win");
      rowA.WinCount.Should().Be(1);

      // HさんはOrangeなので負け
      rowH.Cells[0].IsWinner.Should().BeFalse();
      rowH.WinCount.Should().Be(0);

      // Act 2: Orange Win に変更
      round1.WinningTeam = 2; // Orange

      // Assert 2
      // Aさんは負け
      rowA.Cells[0].IsWinner.Should().BeFalse();
      rowA.WinCount.Should().Be(0);

      // Hさんは勝ち
      rowH.Cells[0].IsWinner.Should().BeTrue();
      rowH.WinCount.Should().Be(1);
    }

    [Fact]
    public void Recalculate_ShouldUpdateRank()
    {
      // Arrange
      // Aさんだけ勝たせる
      // R1: A(Blue) vs H(Orange) -> Blue Win
      var round1 = _viewModel.RoundInputs[0]; // R1

      // Act
      round1.WinningTeam = 1; // Blue Win

      // Assert
      var rowA = _viewModel.PlayerRows.First(p => p.Name == "A");
      var rowH = _viewModel.PlayerRows.First(p => p.Name == "H");

      // Aさんは1勝なので Rank 1
      rowA.WinCount.Should().Be(1);
      rowA.Rank.Should().Be(1);

      // Hさんは0勝なので Rank 2 (または同率下位)
      // ※実装ロジック: 勝数が多い順にソートしてインデックス+1
      // 他の全員0勝なので、A以外は全員 Rank 2 になるはず
      rowH.WinCount.Should().Be(0);
      rowH.Rank.Should().Be(2);
    }

    [Fact]
    public async Task SaveSession_ShouldCallRepository()
    {
      // Arrange
      _viewModel.RoundInputs[0].WinningTeam = 1;

      // Act
      await _viewModel.SaveSessionCommand.ExecuteAsync(null);

      // Assert
      _mockRepo.Verify(r => r.AddSessionAsync(It.Is<ArenaSessionEntity>(s =>
        s.Rounds.Count == 14 &&
        s.PlayerIdsCsv.Contains("A") && // プレイヤー名が含まれているか
        s.Rounds.Any(r => r.RoundNumber == 1 && r.WinningTeam == 1)
      )), Times.Once);

      _viewModel.StatusText.Should().Contain("保存しました");
    }
  }
}
