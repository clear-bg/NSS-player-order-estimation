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
    public partial class RankingView : UserControl
    {
        private readonly RankingRepository _rankingRepo;
        private readonly AliasRepository _aliasRepo;
        private readonly RelationshipExtractor _extractor;
        private readonly OrderSorter _sorter;
        private readonly DbSchemaService _schemaService;

        public RankingView()
        {
            InitializeComponent();

            // 初期化
            _rankingRepo = new RankingRepository();
            _aliasRepo = new AliasRepository();
            _extractor = new RelationshipExtractor();
            _sorter = new OrderSorter();
            _schemaService = new DbSchemaService();

            UpdateEnvironmentDisplay();
            InitializeData();
        }

        private void UpdateEnvironmentDisplay()
        {
            string envName = _rankingRepo.GetEnvironmentName();
            EnvText.Text = envName;

            if (envName == "PROD")
            {
                EnvBadge.Background = Avalonia.Media.Brushes.DarkRed;
            }
            else
            {
                EnvBadge.Background = Avalonia.Media.Brushes.Green;
            }
        }

        private void InitializeData()
        {
            try
            {
                // テーブル作成とデータ読み込み
                _schemaService.EnsureTablesExist();
                LoadRanking();
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
                // 1. エイリアス辞書を使って正規化
                var aliasDict = _aliasRepo.GetAliasDictionary();
                string normalizedInput = _extractor.NormalizeInput(rawInput, aliasDict);

                // 2. ログ保存
                _rankingRepo.AddObservation(normalizedInput);

                // 3. ペア分解
                var pairs = _extractor.ExtractFromInput(normalizedInput);
                if (pairs.Count == 0)
                {
                    StatusText.Text = "⚠️ 有効なペアが見つかりませんでした";
                    return;
                }

                // 4. プレイヤー登録 & 関係更新
                var playerNames = pairs.Select(p => p.Predecessor)
                                       .Concat(pairs.Select(p => p.Successor))
                                       .Distinct();
                _rankingRepo.RegisterPlayers(playerNames);
                _rankingRepo.UpdatePairs(pairs);

                // 5. 結果表示
                if (rawInput != normalizedInput)
                    StatusText.Text = $"✅ 登録完了 (変換あり): \n'{rawInput}' \n→ '{normalizedInput}'";
                else
                    StatusText.Text = $"✅ 登録完了: {pairs.Count} 件の関係を更新しました";

                InputBox.Text = "";
                LoadRanking();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ エラー: {ex.Message}";
            }
        }

        private void ReloadButton_Click(object? sender, RoutedEventArgs e)
        {
            LoadRanking();
        }

        private void LoadRanking()
        {
            try
            {
                var allPairs = _rankingRepo.GetAllPairs();
                var sortedLayers = _sorter.Sort(allPairs);

                var displayList = new List<string>();
                int currentRank = 1;

                foreach (var group in sortedLayers)
                {
                    if (group.Count == 1)
                        displayList.Add($"{currentRank}位 : {group[0]}");
                    else
                        displayList.Add($"{currentRank}位 : {string.Join(", ", group)} (推定同率)");

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
            // ダイアログは Window なので TopLevel を探す必要があるが、
            // UserControl 内でも ShowDialog(window) は使える（親ウィンドウを探してくれる）
            var window = TopLevel.GetTopLevel(this) as Window;
            if (window == null) return;

            var dialog = new ConfirmationDialog();
            var result = await dialog.ShowDialog<bool>(window);

            if (result)
            {
                try
                {
                    _rankingRepo.ClearAllData();
                    StatusText.Text = "🗑️ データを全削除しました";
                    LoadRanking();
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"❌ 削除エラー: {ex.Message}";
                }
            }
        }
    }
}