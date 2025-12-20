using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NssOrderTool.Database;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain;
using NssOrderTool.ViewModels;
using Xunit;

namespace NssOrderTool.Tests.ViewModels
{
  public class ArenaViewModelTests
  {
    private readonly Mock<ArenaRepository> _mockArenaRepo;
    private readonly Mock<PlayerRepository> _mockPlayerRepo; // ★追加
    private readonly ArenaLogicService _logicService;
    private readonly ArenaViewModel _viewModel;

    public ArenaViewModelTests()
    {
      // Repositoryのモック
      _mockArenaRepo = new Mock<ArenaRepository>((AppDbContext)null!);
      _mockPlayerRepo = new Mock<PlayerRepository>((AppDbContext)null!); // ★追加

      // LogicServiceは本物を使用
      _logicService = new ArenaLogicService();

      // ViewModel生成 (依存関係を注入)
      _viewModel = new ArenaViewModel(
          _mockArenaRepo.Object,
          _mockPlayerRepo.Object, // ★追加
          _logicService
      );
    }

    [Fact]
    public void Initialize_ShouldCreate14Rounds_And8Players()
    {
      _viewModel.RoundInputs.Should().HaveCount(14);
      _viewModel.RoundInputs.First().RoundNumber.Should().Be(1);
      _viewModel.PlayerRows.Should().HaveCount(8);
      _viewModel.PlayerRows.First().Name.Should().Be("A");
      _viewModel.PlayerRows.Last().Name.Should().Be("H");
    }

    [Fact]
    public void ToggleWinner_ShouldUpdateRowsAndRanks()
    {
      var round1 = _viewModel.RoundInputs.First(r => r.RoundNumber == 1);
      var rowA = _viewModel.PlayerRows.First(p => p.Name == "A");
      var rowH = _viewModel.PlayerRows.First(p => p.Name == "H");

      // Blue Win
      round1.WinningTeam = 1;
      rowA.WinCount.Should().Be(1);
      rowH.WinCount.Should().Be(0);

      // Orange Win
      round1.WinningTeam = 2;
      rowA.WinCount.Should().Be(0);
      rowH.WinCount.Should().Be(1);
    }

    [Fact]
    public void Recalculate_ShouldUpdateRank()
    {
      var round1 = _viewModel.RoundInputs[0];
      round1.WinningTeam = 1; // Blue Win

      var rowA = _viewModel.PlayerRows.First(p => p.Name == "A");
      var rowH = _viewModel.PlayerRows.First(p => p.Name == "H");

      rowA.Rank.Should().Be(1);
      rowH.Rank.Should().Be(2);
    }

    [Fact]
    public async Task SaveSession_ShouldCallRepositories()
    {
      // Arrange
      _viewModel.RoundInputs[0].WinningTeam = 1;

      // RegisterPlayersAsync が呼ばれたら何もしない（Task完了）ようにセットアップ
      _mockPlayerRepo
          .Setup(r => r.RegisterPlayersAsync(It.IsAny<IEnumerable<string>>()))
          .Returns(Task.CompletedTask);

      // Act
      await _viewModel.SaveSessionCommand.ExecuteAsync(null);

      // Assert
      // 1. プレイヤー登録が呼ばれたか確認
      _mockPlayerRepo.Verify(r => r.RegisterPlayersAsync(
          It.Is<IEnumerable<string>>(names => names.Contains("A") && names.Contains("H"))
      ), Times.Once);

      // 2. セッション保存が呼ばれたか確認 (CSVではなくParticipantsの中身を検証)
      _mockArenaRepo.Verify(r => r.AddSessionAsync(It.Is<ArenaSessionEntity>(s =>
        s.Rounds.Count == 14 &&
        s.Participants.Any(p => p.PlayerId == "A") && // Aさんが含まれているか
        s.Rounds.Any(r => r.RoundNumber == 1 && r.WinningTeam == 1)
      )), Times.Once);

      _viewModel.StatusText.Should().Contain("保存しました");
    }
  }
}
