using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NssOrderTool.Database;
using NssOrderTool.Models;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain;
using NssOrderTool.ViewModels;
using Xunit;

namespace NssOrderTool.Tests.ViewModels
{
    public class SimulationViewModelTests
    {
        private readonly Mock<OrderRepository> _mockOrderRepo;
        private readonly Mock<AliasRepository> _mockAliasRepo;
        private readonly Mock<OrderSorter> _mockSorter;
        private readonly Mock<RelationshipExtractor> _mockExtractor;
        private readonly SimulationViewModel _viewModel;

        public SimulationViewModelTests()
        {
            // 依存関係のモック化
            _mockOrderRepo = new Mock<OrderRepository>((AppDbContext)null!, (AppConfig)null!);
            _mockAliasRepo = new Mock<AliasRepository>((AppDbContext)null!);

            // OrderSorterはロジックが複雑なのでモック化し、
            // Sortメソッドが特定の階層（レイヤー）を返すようにセットアップします
            var mockLogger = new Mock<ILogger<OrderSorter>>();
            _mockSorter = new Mock<OrderSorter>(mockLogger.Object);

            _mockExtractor = new Mock<RelationshipExtractor>();

            // ViewModel初期化
            _viewModel = new SimulationViewModel(
                _mockOrderRepo.Object,
                _mockAliasRepo.Object,
                _mockSorter.Object,
                _mockExtractor.Object
            );
        }

        [Fact]
        public async Task RunSimulation_ShouldSortByRank_AndKeepHostFirst()
        {
            // Arrange
            // 1. モックの設定: ソート結果 (A -> B -> C の順)
            var layers = new List<List<string>>
            {
                new List<string> { "A" }, // Rank 0
                new List<string> { "B" }, // Rank 1
                new List<string> { "C" }  // Rank 2
            };
            _mockOrderRepo.Setup(r => r.GetAllPairsAsync()).ReturnsAsync(new List<OrderPair>());
            _mockSorter.Setup(s => s.Sort(It.IsAny<List<OrderPair>>())).Returns(layers);
            _mockAliasRepo.Setup(r => r.GetAliasDictionaryAsync()).ReturnsAsync(new Dictionary<string, string>());

            // 2. 入力: 3番目のCをホスト(1行目)にし、他をバラバラに入れる
            _viewModel.Inputs[0].Name = "C"; // ホスト (本来は最下位だが1位になるべき)
            _viewModel.Inputs[1].Name = "B"; // 2位になるべき
            _viewModel.Inputs[2].Name = "A"; // 本来は最強だが、ホストより下の2番手になるべき

            // Act
            await _viewModel.RunSimulationCommand.ExecuteAsync(null);

            // Assert
            var results = _viewModel.SimulationResults;
            results.Should().HaveCount(3);

            // 1位: C (ホスト特権)
            results[0].Rank.Should().Be(1);
            results[0].PlayerName.Should().Be("C");
            results[0].IsHost.Should().BeTrue();

            // 2位: A (Bより強い)
            results[1].Rank.Should().Be(2);
            results[1].PlayerName.Should().Be("A");

            // 3位: B
            results[2].Rank.Should().Be(3);
            results[2].PlayerName.Should().Be("B");
        }

        [Fact]
        public async Task RunSimulation_ShouldMarkTiedRanks_AndAssignColors()
        {
            // Arrange
            // 同率（同じレイヤー）を含む構造
            // Rank 0: X
            // Rank 1: Y, Z (同率)
            var layers = new List<List<string>>
            {
                new List<string> { "X" },
                new List<string> { "Y", "Z" }
            };
            _mockOrderRepo.Setup(r => r.GetAllPairsAsync()).ReturnsAsync(new List<OrderPair>());
            _mockSorter.Setup(s => s.Sort(It.IsAny<List<OrderPair>>())).Returns(layers);
            _mockAliasRepo.Setup(r => r.GetAliasDictionaryAsync()).ReturnsAsync(new Dictionary<string, string>());

            _viewModel.Inputs[0].Name = "Host";
            _viewModel.Inputs[1].Name = "X";
            _viewModel.Inputs[2].Name = "Y";
            _viewModel.Inputs[3].Name = "Z";

            // Act
            await _viewModel.RunSimulationCommand.ExecuteAsync(null);

            // Assert
            var results = _viewModel.SimulationResults;

            // 1位: Host
            results[0].Rank.Should().Be(1);

            // 2位: X
            results[1].Rank.Should().Be(2);
            results[1].IsTied.Should().BeFalse();

            // 3位タイ: Y, Z
            var yResult = results[2];
            var zResult = results[3];

            yResult.Rank.Should().Be(3);
            zResult.Rank.Should().Be(3); // 同じランク番号であること

            yResult.IsTied.Should().BeTrue();
            zResult.IsTied.Should().BeTrue();

            // 色の確認: 最初の同率グループなので「スカイブルー (#00BFFF)」であるべき
            // SolidColorBrush同士の比較はColorプロパティで行う
            var expectedColor = SolidColorBrush.Parse("#00BFFF").Color;

            ((SolidColorBrush)yResult.RankColor).Color.Should().Be(expectedColor);
            ((SolidColorBrush)zResult.RankColor).Color.Should().Be(expectedColor);
        }

        [Fact]
        public async Task RunSimulation_ShouldUseDifferentColors_ForMultipleTiedGroups()
        {
            // Arrange
            // 2つの同率グループを作る
            // Rank 0: A, B (同率1組目)
            // Rank 1: C, D (同率2組目)
            var layers = new List<List<string>>
            {
                new List<string> { "A", "B" },
                new List<string> { "C", "D" }
            };
            _mockOrderRepo.Setup(r => r.GetAllPairsAsync()).ReturnsAsync(new List<OrderPair>());
            _mockSorter.Setup(s => s.Sort(It.IsAny<List<OrderPair>>())).Returns(layers);
            _mockAliasRepo.Setup(r => r.GetAliasDictionaryAsync()).ReturnsAsync(new Dictionary<string, string>());

            _viewModel.Inputs[0].Name = "Host";
            _viewModel.Inputs[1].Name = "A";
            _viewModel.Inputs[2].Name = "B";
            _viewModel.Inputs[3].Name = "C";
            _viewModel.Inputs[4].Name = "D";

            // Act
            await _viewModel.RunSimulationCommand.ExecuteAsync(null);

            // Assert
            var results = _viewModel.SimulationResults;

            // 1組目 (A, B) -> 2位タイ -> スカイブルー (#00BFFF)
            var color1 = ((SolidColorBrush)results[1].RankColor).Color;
            color1.Should().Be(SolidColorBrush.Parse("#00BFFF").Color);

            // 2組目 (C, D) -> 4位タイ -> 黄緑 (#9ACD32)
            var color2 = ((SolidColorBrush)results[3].RankColor).Color;
            color2.Should().Be(SolidColorBrush.Parse("#9ACD32").Color);
        }
    }
}