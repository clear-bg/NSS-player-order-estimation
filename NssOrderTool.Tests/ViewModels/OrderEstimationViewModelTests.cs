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
        // ロジッククラスは本物を使う（計算が正しいかも含めてテストしたい場合）
        // または、ロジッククラスもインターフェース化してモックにすることも可能ですが、
        // 今回は「計算ロジック」のテストは済んでいるので本物を使います。
        private readonly RelationshipExtractor _extractor;
        private readonly OrderSorter _sorter;
        private readonly Mock<DbSchemaService> _mockSchemaService;
        private readonly Mock<ILogger<OrderEstimationViewModel>> _mockLogger;

        public OrderEstimationViewModelTests()
        {
            // モックの初期化 (DbManagerは使わないのでnull)
            _mockOrderRepo = new Mock<OrderRepository>((DbManager)null!);
            _mockPlayerRepo = new Mock<PlayerRepository>((DbManager)null!);
            _mockAliasRepo = new Mock<AliasRepository>((DbManager)null!);
            _mockSchemaService = new Mock<DbSchemaService>((DbManager)null!);
            _mockLogger = new Mock<ILogger<OrderEstimationViewModel>>();

            // ロジッククラスは本物を使用
            _extractor = new RelationshipExtractor();
            _sorter = new OrderSorter();
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
            // GetAllPairsAsyncが呼ばれ、結果がRankingListに反映されているはず
            _mockOrderRepo.Verify(r => r.GetAllPairsAsync(), Times.AtLeastOnce);

            viewModel.RankingList.Should().NotBeEmpty();
            viewModel.RankingList[0].Should().Contain("PlayerA"); // 1位
            viewModel.RankingList[1].Should().Contain("PlayerB"); // 2位

            // F. 入力欄がクリアされたか？
            viewModel.InputText.Should().BeEmpty();
        }
    }
}