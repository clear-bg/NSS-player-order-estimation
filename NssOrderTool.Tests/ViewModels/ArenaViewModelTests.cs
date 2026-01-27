using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NssOrderTool.Database;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain;
using NssOrderTool.Services.Rating;
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
      // 1. ArenaRepository のモック (引数2つ: DbContext, IServiceProvider)
      _mockArenaRepo = new Mock<ArenaRepository>((AppDbContext)null!, (System.IServiceProvider)null!);

      // 2. PlayerRepository のモック (引数1つ: DbContext)
      _mockPlayerRepo = new Mock<PlayerRepository>((AppDbContext)null!);

      // 3. LogicService の初期化
      var mockCalculator = new Mock<IRatingCalculator>();

      // ★修正: 3つ目の引数として _mockArenaRepo.Object を渡す
      _realLogicService = new ArenaLogicService(mockCalculator.Object, _mockPlayerRepo.Object, _mockArenaRepo.Object);
    }

    [Fact]
    public async Task SaveSession_ShouldCallRepositories_WhenExecuted()
    {
      // Arrange
      var vm = new ArenaViewModel(_mockArenaRepo.Object, _mockPlayerRepo.Object, _realLogicService);

      // テストデータをセット (保存対象のプレイヤー名など)
      vm.PlayerRows.First().Name = "TestPlayer";
      vm.PlayerRows.First().WinCount = 5;

      // Act
      await vm.SaveSessionCommand.ExecuteAsync(null);

      // Assert
      // 1. プレイヤー登録が呼ばれたか検証
      _mockPlayerRepo.Verify(r => r.RegisterPlayersAsync(It.IsAny<IEnumerable<string>>()), Times.Once);

      // 2. セッション保存が呼ばれたか検証
      _mockArenaRepo.Verify(r => r.AddSessionAsync(It.IsAny<ArenaSessionEntity>()), Times.Once);

      vm.StatusText.Should().Contain("保存");
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
