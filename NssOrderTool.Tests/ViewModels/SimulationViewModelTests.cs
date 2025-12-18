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
using CommunityToolkit.Mvvm.Messaging;

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
            _mockOrderRepo = new Mock<OrderRepository>((AppDbContext)null!, (AppConfig)null!);
            _mockAliasRepo = new Mock<AliasRepository>((AppDbContext)null!);

            var mockLogger = new Mock<ILogger<OrderSorter>>();
            _mockSorter = new Mock<OrderSorter>(mockLogger.Object);

            _mockExtractor = new Mock<RelationshipExtractor>();

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
            var layers = new List<List<string>>
            {
                new List<string> { "A" },
                new List<string> { "B" },
                new List<string> { "C" }
            };
            _mockOrderRepo.Setup(r => r.GetAllPairsAsync()).ReturnsAsync(new List<OrderPair>());
            _mockSorter.Setup(s => s.Sort(It.IsAny<List<OrderPair>>())).Returns(layers);
            _mockAliasRepo.Setup(r => r.GetAliasDictionaryAsync()).ReturnsAsync(new Dictionary<string, string>());

            _viewModel.Inputs[0].Name = "C";
            _viewModel.Inputs[1].Name = "B";
            _viewModel.Inputs[2].Name = "A";

            await _viewModel.RunSimulationCommand.ExecuteAsync(null);

            var results = _viewModel.SimulationResults;
            results.Should().HaveCount(3);
            results[0].PlayerName.Should().Be("C");
            results[1].PlayerName.Should().Be("A");
            results[2].PlayerName.Should().Be("B");
        }

        [Fact]
        public async Task RunSimulation_ShouldMarkTiedRanks_AndAssignColors()
        {
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

            await _viewModel.RunSimulationCommand.ExecuteAsync(null);

            var results = _viewModel.SimulationResults;
            ((SolidColorBrush)results[2].RankColor).Color.Should().Be(SolidColorBrush.Parse("#00BFFF").Color);
        }

        [Fact]
        public async Task RunSimulation_ShouldUseDifferentColors_ForMultipleTiedGroups()
        {
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

            await _viewModel.RunSimulationCommand.ExecuteAsync(null);

            var results = _viewModel.SimulationResults;
            ((SolidColorBrush)results[1].RankColor).Color.Should().Be(SolidColorBrush.Parse("#00BFFF").Color);
            ((SolidColorBrush)results[3].RankColor).Color.Should().Be(SolidColorBrush.Parse("#9ACD32").Color);
        }

        // ▼▼▼ 追加したテスト (修正済み) ▼▼▼

        [Fact]
        public async Task Initialize_ShouldLoadUniquePlayerNames_FromRepository()
        {
            // Arrange
            var pairs = new List<OrderPair>
            {
                // 修正: コンストラクタ引数で指定
                new OrderPair("A", "B"),
                new OrderPair("B", "C")
            };
            
            _mockOrderRepo.Setup(r => r.GetAllPairsAsync()).ReturnsAsync(pairs);
            
            var vm = new SimulationViewModel(
                _mockOrderRepo.Object, 
                _mockAliasRepo.Object, 
                _mockSorter.Object, 
                _mockExtractor.Object);

            // ロード待ち
            await Task.Delay(50); 

            // Assert
            vm.AllPlayerNames.Should().HaveCount(3);
            vm.AllPlayerNames.Should().Contain(new[] { "A", "B", "C" });
        }
        
        // [Fact]
        // public async Task ReceiveUpdateMessage_ShouldReloadPlayerNames()
        // {
        //     // Arrange
        //     var pairsInitial = new List<OrderPair> { new OrderPair("Old", "Boy") };
        //     var pairsUpdated = new List<OrderPair> { new OrderPair("New", "Comer") };

        //     _mockOrderRepo.SetupSequence(r => r.GetAllPairsAsync())
        //         .ReturnsAsync(pairsInitial)
        //         .ReturnsAsync(pairsUpdated);

        //     var vm = new SimulationViewModel(
        //         _mockOrderRepo.Object,
        //         _mockAliasRepo.Object,
        //         _mockSorter.Object,
        //         _mockExtractor.Object);

        //     // 初回ロード完了を簡易的に待つ（ここは同期的に近い動きをするためDelayでも比較的安全）
        //     await Task.Delay(100); 
        //     vm.AllPlayerNames.Should().Contain("Old");

        //     // ▼▼▼ 修正: イベント監視による待機 ▼▼▼
        //     // 「コレクションが変更されたら完了とする」タスクを作成
        //     var tcs = new TaskCompletionSource<bool>();
            
        //     void Handler(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //     {
        //         // "New" が追加されたら完了とみなす
        //         if (vm.AllPlayerNames.Contains("New"))
        //         {
        //             tcs.TrySetResult(true);
        //         }
        //     }

        //     vm.AllPlayerNames.CollectionChanged += Handler;

        //     try
        //     {
        //         // Act: メッセージ送信
        //         WeakReferenceMessenger.Default.Send(new NssOrderTool.Messages.DatabaseUpdatedMessage());

        //         // タイムアウト付きで変更イベントを待つ (最大1秒)
        //         var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(1000));

        //         if (completedTask != tcs.Task)
        //         {
        //             // タイムアウトした場合
        //             throw new System.TimeoutException("The player list was not updated within the timeout period.");
        //         }
        //     }
        //     finally
        //     {
        //         // イベント購読の解除 (メモリリーク防止)
        //         vm.AllPlayerNames.CollectionChanged -= Handler;
        //     }

        //     // Assert
        //     vm.AllPlayerNames.Should().Contain("New");
        //     vm.AllPlayerNames.Should().Contain("Comer");
        // }
    }
}