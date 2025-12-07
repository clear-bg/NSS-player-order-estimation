using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain;
using NssOrderTool.Database;
using NssOrderTool.Models;

namespace NssOrderTool.Views
{
    // クラス名変更
    public partial class OrderEstimationView : UserControl
    {
        // フィールド名と型変更: _rankingRepo -> _orderRepo
        private readonly OrderRepository _orderRepo;
        private readonly AliasRepository _aliasRepo;
        private readonly PlayerRepository _playerRepo;
        private readonly RelationshipExtractor _extractor;
        private readonly OrderSorter _sorter;
        private readonly DbSchemaService _schemaService;

        public OrderEstimationView()
        {
            InitializeComponent();

            // 初期化
            _orderRepo = new OrderRepository();
            _aliasRepo = new AliasRepository();
            _playerRepo = new PlayerRepository();
            _extractor = new RelationshipExtractor();
            _sorter = new OrderSorter();
            _schemaService = new DbSchemaService();

            UpdateEnvironmentDisplay();
            InitializeData();
        }

        private void UpdateEnvironmentDisplay()
        {
            string envName = _orderRepo.GetEnvironmentName();
            EnvText.Text = envName;

            if (envName == "PROD")
                EnvBadge.Background = Avalonia.Media.Brushes.DarkRed;
            else
                EnvBadge.Background = Avalonia.Media.Brushes.Green;
        }

        private void InitializeData()
        {
            try
            {
                _schemaService.EnsureTablesExist();
                LoadOrder(); // メソッド名も変更推奨 (LoadRanking -> LoadOrder)
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ 初期化エラー: {ex.Message}";
            }
        }

        private void RegisterButton_Click(object? sender, RoutedEventArgs e)
        {
            string rawInput = InputBox.Text ?? "";
            if (string.IsNullOrWhiteSpace(rawInput))
            {
                StatusText.Text = "⚠️ 入力が空です";
                return;
            }

            try
            {
                var aliasDict = _aliasRepo.GetAliasDictionary();
                string normalizedInput = _extractor.NormalizeInput(rawInput, aliasDict);

                // _orderRepo を使用
                _orderRepo.AddObservation(normalizedInput);

                var pairs = _extractor.ExtractFromInput(normalizedInput);
                if (pairs.Count == 0)
                {
                    StatusText.Text = "⚠️ 有効なペアが見つかりませんでした";
                    return;
                }

                var playerNames = pairs.Select(p => p.Predecessor)
                                       .Concat(pairs.Select(p => p.Successor))
                                       .Distinct();
                _playerRepo.RegisterPlayers(playerNames);
                _orderRepo.UpdatePairs(pairs);

                if (rawInput != normalizedInput)
                    StatusText.Text = $"✅ 登録完了 (変換あり): \n'{rawInput}' \n→ '{normalizedInput}'";
                else
                    StatusText.Text = $"✅ 登録完了: {pairs.Count} 件の関係を更新しました";

                InputBox.Text = "";
                LoadOrder();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ エラー: {ex.Message}";
            }
        }

        private void ReloadButton_Click(object? sender, RoutedEventArgs e)
        {
            LoadOrder();
        }

        // メソッド名変更: LoadRanking -> LoadOrder
        private void LoadOrder()
        {
            try
            {
                var allPairs = _orderRepo.GetAllPairs();
                var sortedLayers = _sorter.Sort(allPairs);

                var displayList = new List<string>();
                int currentRank = 1;

                foreach (var group in sortedLayers)
                {
                    if (group.Count == 1)
                        displayList.Add($"{currentRank} : {group[0]}"); // "位" を削除しても良いかも
                    else
                        displayList.Add($"{currentRank} : {string.Join(", ", group)} (推定同列)");

                    currentRank++;
                }
                RankingList.ItemsSource = displayList;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ 読み込みエラー: {ex.Message}";
            }
        }

        private async void ClearButton_Click(object? sender, RoutedEventArgs e)
        {
            var window = TopLevel.GetTopLevel(this) as Window;
            if (window == null) return;

            var dialog = new ConfirmationDialog();
            var result = await dialog.ShowDialog<bool>(window);

            if (result)
            {
                try
                {
                    _orderRepo.ClearAllData();
                    _playerRepo.ClearAll();
                    _aliasRepo.ClearAll();

                    StatusText.Text = "🗑️ データを全削除しました";
                    LoadOrder();
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"❌ 削除エラー: {ex.Message}";
                }
            }
        }
    }
}