using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NssOrderTool.Database; // AppDbContextのために必要
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
    private readonly Mock<PlayerRepository> _mockPlayerRepo;
    private readonly ArenaLogicService _realLogicService;

    public ArenaViewModelTests()
    {
      // Repositoryは仮想(Mock)化する
      // Repositoryのコンストラクタ引数(AppDbContext)に合わせて、nullを1つだけ渡す
      _mockArenaRepo = new Mock<ArenaRepository>((AppDbContext)null!);

      // ★修正箇所: 引数を (null!, null!) から (null!) に変更
      _mockPlayerRepo = new Mock<PlayerRepository>((AppDbContext)null!);

      _realLogicService = new ArenaLogicService();
    }

    [Fact]
    public async Task SaveSession_ShouldCallRepositories_WhenExecuted()
    {
      // Arrange
      var vm = new ArenaViewModel(_mockArenaRepo.Object, _mockPlayerRepo.Object, _realLogicService);

      vm.PlayerRows.First().WinCount = 5;

      // Act
      await vm.SaveSessionCommand.ExecuteAsync(null);

      // Assert
      // 1. プレイヤー登録(RegisterPlayersAsync)が呼ばれたか検証
      _mockPlayerRepo.Verify(r => r.RegisterPlayersAsync(It.IsAny<List<string>>()), Times.Once);

      // 2. セッション保存(AddSessionAsync)が呼ばれたか検証
      _mockArenaRepo.Verify(r => r.AddSessionAsync(It.IsAny<ArenaSessionEntity>()), Times.Once);

      vm.StatusText.Should().Contain("保存しました");
    }

    [Fact]
    public void ChangingRoundInput_ShouldTriggerRecalculate()
    {
      // Arrange
      var vm = new ArenaViewModel(_mockArenaRepo.Object, _mockPlayerRepo.Object, _realLogicService);

      vm.PlayerRows.All(p => p.Rank == 1).Should().BeTrue();

      // Act
      // ラウンド1で「Blueチーム(TeamId=1)」が勝ったことにする
      vm.RoundInputs[0].WinningTeam = 1;

      // Assert
      // Aさん(Index=0)は勝っているはず -> 勝利数1
      var playerA = vm.PlayerRows.First(p => p.Index == 0);
      playerA.WinCount.Should().Be(1);

      // Eさん(Index=4)は負けているはず -> 勝利数0
      var playerE = vm.PlayerRows.First(p => p.Index == 4);
      playerE.WinCount.Should().Be(0);

      playerA.Rank.Should().Be(1);
      playerE.Rank.Should().Be(5);
    }
  }
}
