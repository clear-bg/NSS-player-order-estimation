using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NssOrderTool.Database;
using NssOrderTool.Models;
using NssOrderTool.Repositories;
using NssOrderTool.ViewModels;
using Xunit;

namespace NssOrderTool.Tests.ViewModels
{
  public class AliasSettingsViewModelTests
  {
    private readonly Mock<AliasRepository> _mockRepo;
    private readonly Mock<OrderRepository> _mockOrderRepo;
    private readonly AliasSettingsViewModel _viewModel;

    public AliasSettingsViewModelTests()
    {
      // AliasRepositoryのモック作成
      _mockRepo = new Mock<AliasRepository>((AppDbContext)null!);

      _mockRepo.Setup(r => r.GetAliasDictionaryAsync())
               .ReturnsAsync(new Dictionary<string, string>());

      _mockOrderRepo = new Mock<OrderRepository>((AppDbContext)null!, (AppConfig)null!);

      // ViewModelの初期化
      _viewModel = new AliasSettingsViewModel(_mockRepo.Object, _mockOrderRepo.Object);
    }

    [Fact]
    public async Task AddAliasCommand_ShouldReportError_WhenAliasEqualsTarget()
    {
      // Arrange
      _viewModel.TargetInput = "Takahiro";
      _viewModel.AliasInput = "Takahiro"; // 正規名と同じ名前を入力

      // Act
      await _viewModel.AddAliasCommand.ExecuteAsync(null);

      // Assert
      // 1. リポジトリのAddメソッドは呼ばれてはいけない
      _mockRepo.Verify(r => r.AddAliasAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

      // 2. ステータスにエラーが含まれていること
      _viewModel.StatusText.Should().Contain("正規名と同じ");
    }

    [Fact]
    public async Task AddAliasCommand_ShouldCallRepository_WhenInputIsValid()
    {
      // Arrange
      _viewModel.TargetInput = "Takahiro";
      _viewModel.AliasInput = "Taka"; // 有効なエイリアス

      // Act
      await _viewModel.AddAliasCommand.ExecuteAsync(null);

      // Assert
      // リポジトリが正しい引数で呼ばれたか確認
      _mockRepo.Verify(r => r.AddAliasAsync("Taka", "Takahiro"), Times.Once);

      // 成功メッセージが出ているか確認
      _viewModel.StatusText.Should().Contain("追加しました");
    }

    [Fact]
    public async Task AddAliasCommand_ShouldCallMergePlayerIds()
    {
      // Arrange
      _viewModel.TargetInput = "Takahiro";
      _viewModel.AliasInput = "Taka";

      // Act
      await _viewModel.AddAliasCommand.ExecuteAsync(null);

      // Assert
      // AliasRepositoryへの追加呼び出し確認
      _mockRepo.Verify(r => r.AddAliasAsync("Taka", "Takahiro"), Times.Once);

      // 【重要】OrderRepositoryのMergePlayerIdsAsyncが呼ばれたか確認
      _mockOrderRepo.Verify(r => r.MergePlayerIdsAsync("Taka", "Takahiro"), Times.Once);
    }
  }
}