using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NssOrderTool.Database;
using NssOrderTool.Repositories;
using NssOrderTool.ViewModels;
using Xunit;

namespace NssOrderTool.Tests.ViewModels
{
  public class AliasEditViewModelTests
  {
    [Fact]
    public async Task LoadAliasesAsync_ShouldPopulateAliases_FromRepository()
    {
      // --- 1. Arrange (準備) ---
      var targetName = "Takahiro";
      var expectedAliases = new List<string> { "Taka", "Tak", "T.K" };

      // Mockの作成
      // コンストラクタ引数が AppDbContext に変わったので、(AppDbContext)null! を渡します
      var mockRepo = new Mock<AliasRepository>((AppDbContext)null!);

      // Setup: 「GetAliasesByTargetAsync("Takahiro") が呼ばれたら、expectedAliases を返せ」と教える
      mockRepo.Setup(r => r.GetAliasesByTargetAsync(targetName))
              .ReturnsAsync(expectedAliases);

      // ViewModelにモックを渡して生成
      var viewModel = new AliasEditViewModel(targetName, mockRepo.Object);

      // --- 2. Act (実行) ---
      await viewModel.LoadAliasesAsync();

      // --- 3. Assert (検証) ---
      viewModel.Aliases.Should().HaveCount(3);
      viewModel.Aliases.Should().ContainInOrder(expectedAliases);
    }

    [Fact]
    public async Task DeleteAlias_ShouldCallRepository_AndReload()
    {
      // --- 1. Arrange ---
      var targetName = "Takahiro";
      var aliasToDelete = "BadAlias";

      // コンストラクタ引数を AppDbContext に修正
      var mockRepo = new Mock<AliasRepository>((AppDbContext)null!);

      // Setup: 削除後の再読み込みで空リストを返すようにしておく
      mockRepo.Setup(r => r.GetAliasesByTargetAsync(targetName))
              .ReturnsAsync(new List<string>());

      var viewModel = new AliasEditViewModel(targetName, mockRepo.Object);

      // --- 2. Act ---
      // "BadAlias" を削除するコマンドを実行
      await viewModel.DeleteAlias(aliasToDelete);

      // --- 3. Assert ---
      // 「リポジトリの DeleteAliasAsync("BadAlias") が、1回だけ呼ばれたこと」を検証
      mockRepo.Verify(r => r.DeleteAliasAsync(aliasToDelete), Times.Once);

      // 削除後に再読み込み(LoadAliasesAsync)も呼ばれていることを検証
      mockRepo.Verify(r => r.GetAliasesByTargetAsync(targetName), Times.AtLeastOnce);
    }
  }
}