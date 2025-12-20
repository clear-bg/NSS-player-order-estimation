using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NssOrderTool.Database;
using NssOrderTool.Models;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain;
using NssOrderTool.ViewModels;
using Xunit;

namespace NssOrderTool.Tests.ViewModels
{
  public class OrderEstimationViewModelTests
  {
    // モックオブジェクト
    private readonly Mock<OrderRepository> _mockOrderRepo;
    private readonly Mock<PlayerRepository> _mockPlayerRepo;
    private readonly Mock<AliasRepository> _mockAliasRepo;
    private readonly Mock<DbSchemaService> _mockSchemaService;
    private readonly Mock<ILogger<OrderEstimationViewModel>> _mockLogger;

    // ロジッククラス（実体を使用）
    private readonly RelationshipExtractor _extractor;
    private readonly OrderSorter _sorter;
    private readonly GraphVizService _graphViz;

    public OrderEstimationViewModelTests()
    {
      // Repositoryは具象クラスなので、コンストラクタ引数に合わせてダミー(null)を渡してMock化
      // (virtualメソッドがモック対象になります)
      _mockOrderRepo = new Mock<OrderRepository>((AppDbContext)null!, (AppConfig)null!);
      _mockPlayerRepo = new Mock<PlayerRepository>((AppDbContext)null!);
      _mockAliasRepo = new Mock<AliasRepository>((AppDbContext)null!);

      _mockSchemaService = new Mock<DbSchemaService>((AppDbContext)null!);
      _mockLogger = new Mock<ILogger<OrderEstimationViewModel>>();

      // ロジッククラスの初期化
      _extractor = new RelationshipExtractor();
      // OrderSorterにもLoggerが必要
      var mockSorterLogger = new Mock<ILogger<OrderSorter>>();
      _sorter = new OrderSorter(mockSorterLogger.Object);
      _graphViz = new GraphVizService();
    }

    [Fact]
    public async Task RegisterCommand_ShouldNormalize_RegisterData_AndReload()
    {
      // Arrange
      var inputText = "Taka, PlayerB";

      // 1. Alias辞書のモック設定
      _mockAliasRepo.Setup(r => r.GetAliasDictionaryAsync())
                    .ReturnsAsync(new Dictionary<string, string> { { "Taka", "PlayerA" } });

      // 2. ペア取得のモック設定（リロード用）
      // 登録後に (PlayerA -> PlayerB) のペアが存在する状態をシミュレート
      var updatedPairs = new List<OrderPair> { new OrderPair("PlayerA", "PlayerB") };
      _mockOrderRepo.Setup(r => r.GetAllPairsAsync())
                    .ReturnsAsync(updatedPairs);

      // 3. 履歴取得のモック設定（リロード用）
      _mockOrderRepo.Setup(r => r.GetRecentObservationsAsync(It.IsAny<int>()))
                    .ReturnsAsync(new List<ObservationEntity>());

      // ViewModel生成
      var viewModel = new OrderEstimationViewModel(
          _mockOrderRepo.Object,
          _mockPlayerRepo.Object,
          _mockAliasRepo.Object,
          _extractor,
          _sorter,
          _mockSchemaService.Object,
          _graphViz,
          _mockLogger.Object
      );

      viewModel.InputText = inputText;

      // Act
      await viewModel.RegisterCommand.ExecuteAsync(null);

      // Assert
      // A. 正規化された文字列で登録メソッドが呼ばれたか
      _mockOrderRepo.Verify(r => r.AddObservationAsync("PlayerA, PlayerB"), Times.Once);

      // B. プレイヤー登録が呼ばれたか
      _mockPlayerRepo.Verify(r => r.RegisterPlayersAsync(It.Is<IEnumerable<string>>(
          list => list.Contains("PlayerA") && list.Contains("PlayerB")
      )), Times.Once);

      // C. ペア更新が呼ばれたか
      _mockOrderRepo.Verify(r => r.UpdatePairsAsync(It.Is<List<OrderPair>>(
          pairs => pairs.Count == 1 && pairs[0].Predecessor == "PlayerA" && pairs[0].Successor == "PlayerB"
      )), Times.Once);

      // D. 画面リストが更新されたか（推定順序の確認）
      viewModel.EstimatedSequence.Should().NotBeEmpty();
      viewModel.EstimatedSequence[0].Should().Contain("PlayerA");
    }

    [Fact]
    public async Task LoadHistoryAsync_ShouldPopulateHistoryList_FromDetails()
    {
      // Arrange
      // 詳細データを含むEntityを作成
      var observation = new ObservationEntity
      {
        Id = 1,
        Details = new List<ObservationDetailEntity>
        {
          new ObservationDetailEntity { PlayerId = "A", OrderIndex = 0 },
          new ObservationDetailEntity { PlayerId = "B", OrderIndex = 1 }
        }
      };

      // GetRecentObservationsAsyncが呼ばれたら、上記データを返す
      _mockOrderRepo.Setup(r => r.GetRecentObservationsAsync(It.IsAny<int>()))
                    .ReturnsAsync(new List<ObservationEntity> { observation });

      // GetAllPairsAsyncは空でOK
      _mockOrderRepo.Setup(r => r.GetAllPairsAsync())
                    .ReturnsAsync(new List<OrderPair>());

      var viewModel = new OrderEstimationViewModel(
          _mockOrderRepo.Object,
          _mockPlayerRepo.Object,
          _mockAliasRepo.Object,
          _extractor,
          _sorter,
          _mockSchemaService.Object,
          _graphViz,
          _mockLogger.Object
      );

      // Act
      // InitializeAsyncはコンストラクタで呼ばれるが、モック設定後に手動でリロード呼出も可
      await viewModel.ReloadCommand.ExecuteAsync(null);

      // Assert
      viewModel.HistoryList.Should().HaveCount(1);
      // Detailsから文字列 "A, B" が復元されているか
      viewModel.HistoryList[0].Content.Should().Be("A, B");
    }
  }
}
