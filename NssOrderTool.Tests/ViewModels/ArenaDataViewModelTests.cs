using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NssOrderTool.Models;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;
using NssOrderTool.ViewModels;
using Xunit;

namespace NssOrderTool.Tests.ViewModels
{
  public class ArenaDataViewModelTests
  {
    [Fact]
    public async Task LoadPlayersAsync_PopulatesTopRanking()
    {
      // Arrange
      // PlayerRepository は AppDbContext 1つを受け取る
      var mockPlayerRepo = new Mock<PlayerRepository>(null!);

      // ★修正箇所: ArenaRepository は (AppDbContext, IServiceProvider) の2つを受け取るため、nullを2つ渡す
      var mockArenaRepo = new Mock<ArenaRepository>(null!, null!);

      var mockConfig = new AppConfig();

      // モックの設定: 上位プレイヤーを返すようにする
      var dummyPlayers = new List<PlayerEntity>
            {
                new PlayerEntity { Id = "P1", Name = "Player1", RateMean = 28.0, RateSigma = 1.0 } // 28 - 3 = 25
            };

      // PlayerRepositoryのメソッドは virtual になっている前提
      mockPlayerRepo.Setup(r => r.GetAllPlayersAsync())
          .ReturnsAsync(new List<PlayerEntity>()); // 検索用リストは空でOK

      mockPlayerRepo.Setup(r => r.GetTopRatedPlayersAsync(It.IsAny<int>()))
          .ReturnsAsync(dummyPlayers);

      var viewModel = new ArenaDataViewModel(mockPlayerRepo.Object, mockArenaRepo.Object, mockConfig);

      // Act
      await viewModel.LoadPlayersAsync();

      // Assert
      Assert.Single(viewModel.TopRanking);
      var item = viewModel.TopRanking[0];
      Assert.Equal(1, item.Rank);
      Assert.Equal("Player1", item.Name);
      Assert.Equal("25", item.Rating); // "F0" フォーマット (28 - 3*1 = 25)
    }
  }
}
