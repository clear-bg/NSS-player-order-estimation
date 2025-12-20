using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NssOrderTool.Database;
using NssOrderTool.Models;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain;
using NssOrderTool.ViewModels;
using Xunit;
using Microsoft.Extensions.Logging;

namespace NssOrderTool.Tests.ViewModels
{
  public class OrderEstimationViewModelTests
  {
    // 依存するモックたち
    private readonly Mock<OrderRepository> _mockOrderRepo;
    private readonly Mock<PlayerRepository> _mockPlayerRepo;
    private readonly Mock<AliasRepository> _mockAliasRepo;
    private readonly RelationshipExtractor _extractor;
    private readonly OrderSorter _sorter;
    private readonly GraphVizService _graphViz;
    private readonly Mock<DbSchemaService> _mockSchemaService;
    private readonly Mock<ILogger<OrderEstimationViewModel>> _mockLogger;
    private readonly Mock<ILogger<OrderSorter>> _mockSorterLogger;

    public OrderEstimationViewModelTests()
    {
      _mockOrderRepo = new Mock<OrderRepository>((AppDbContext)null!, (AppConfig)null!);
      _mockPlayerRepo = new Mock<PlayerRepository>((AppDbContext)null!);
      _mockAliasRepo = new Mock<AliasRepository>((AppDbContext)null!);
      _mockSchemaService = new Mock<DbSchemaService>((AppDbContext)null!);
      _mockLogger = new Mock<ILogger<OrderEstimationViewModel>>();
      _mockSorterLogger = new Mock<ILogger<OrderSorter>>();

      // ロジッククラスは本物を使用
      _extractor = new RelationshipExtractor();
      _sorter = new OrderSorter(_mockSorterLogger.Object);
      _graphViz = new GraphVizService();
    }

    [Fact]
    public async Task RegisterCommand_ShouldNormalize_RegisterData_AndReload()
    {
      // --- 1. Arrange (準備) ---

      // 入力値: エイリアス(Taka)が含まれている
      var inputText = "Taka, PlayerB";

      // モックの設定: エイリアス辞書を返す
      _mockAliasRepo.Setup(r => r.GetAliasDictionaryAsync())
                    .ReturnsAsync(new Dictionary<string, string> { { "Taka", "PlayerA" } });

      // モックの設定: 現在の順序データを返す (再読み込み用)
      // 登録によって (PlayerA -> PlayerB) の関係が生まれると想定し、それを返すように設定
      var updatedPairs = new List<OrderPair> { new OrderPair("PlayerA", "PlayerB") };
      _mockOrderRepo.Setup(r => r.GetAllPairsAsync())
                    .ReturnsAsync(updatedPairs);

      // ViewModelの生成
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

      // 画面入力のシミュレーション
      viewModel.InputText = inputText;

      // --- 2. Act (実行) ---
      // Registerコマンドを実行 (非同期)
      await viewModel.RegisterCommand.ExecuteAsync(null);

      // --- 3. Assert (検証) ---

      // A. エイリアス辞書が取得されたか？
      _mockAliasRepo.Verify(r => r.GetAliasDictionaryAsync(), Times.Once);

      // B. 正規化されたログ "PlayerA, PlayerB" が保存されたか？
      _mockOrderRepo.Verify(r => r.AddObservationAsync("PlayerA, PlayerB"), Times.Once);

      // C. プレイヤー登録が呼ばれたか？ (AとBの2名)
      _mockPlayerRepo.Verify(r => r.RegisterPlayersAsync(It.Is<IEnumerable<string>>(
          list => list.Contains("PlayerA") && list.Contains("PlayerB")
      )), Times.Once);

      // D. 関係性の更新が呼ばれたか？ (A->B のペア)
      _mockOrderRepo.Verify(r => r.UpdatePairsAsync(It.Is<List<OrderPair>>(
          pairs => pairs.Count == 1 && pairs[0].Predecessor == "PlayerA" && pairs[0].Successor == "PlayerB"
      )), Times.Once);

      // E. 画面のリストが更新されたか？ (Reload処理)
      // GetAllPairsAsyncが呼ばれ、結果がEstimatedSequenceに反映されているはず
      _mockOrderRepo.Verify(r => r.GetAllPairsAsync(), Times.AtLeastOnce);

      viewModel.EstimatedSequence.Should().NotBeEmpty();
      viewModel.EstimatedSequence[0].Should().Contain("PlayerA"); // 1位
      viewModel.EstimatedSequence[1].Should().Contain("PlayerB"); // 2位

      // F. 入力欄がクリアされたか？
      viewModel.InputText.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterCommand_ShouldToggleIsBusy_WhileExecuting()
    {
      // Arrange
      // エイリアス辞書は空でOK
      _mockAliasRepo.Setup(r => r.GetAliasDictionaryAsync())
                    .ReturnsAsync(new Dictionary<string, string>());

      // 重い処理をシミュレート: GetAllPairsAsync が呼ばれたら 500ms 待機して空リストを返す
      _mockOrderRepo.Setup(r => r.GetAllPairsAsync())
                    .Returns(async () =>
                    {
                      await Task.Delay(500);
                      return new List<OrderPair>();
                    });

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

      // 入力値をセット（バリデーション回避）
      viewModel.InputText = "A, B";

      // Act
      // コマンド実行開始（awaitせずにタスクを保持）
      var executionTask = viewModel.RegisterCommand.ExecuteAsync(null);

      // Assert 1: 実行中は IsBusy が true であること
      viewModel.IsBusy.Should().BeTrue("非同期処理中はIsBusyがtrueになるべき");

      // 完了まで待機
      await executionTask;

      // Assert 2: 完了後は IsBusy が false に戻ること
      viewModel.IsBusy.Should().BeFalse("処理完了後はIsBusyがfalseに戻るべき");
    }

    [Fact]
    public async Task ReloadCommand_ShouldUpdateStatsText()
    {
      // Arrange
      // 3つのペア、4人のプレイヤー (A->B, B->C, C->D)
      var pairs = new List<OrderPair>
            {
                new OrderPair("A", "B"),
                new OrderPair("B", "C"),
                new OrderPair("C", "D")
            };

      _mockOrderRepo.Setup(r => r.GetAllPairsAsync()).ReturnsAsync(pairs);

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
      await viewModel.ReloadCommand.ExecuteAsync(null);

      // Assert
      // StatsText が "(4 players, 3 pairs)" のようになっているか確認
      viewModel.StatsText.Should().Contain("4 players");
      viewModel.StatsText.Should().Contain("3 pairs");
    }
  }
}
