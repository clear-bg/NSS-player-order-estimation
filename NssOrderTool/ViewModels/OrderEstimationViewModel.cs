using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NssOrderTool.Database;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain;

namespace NssOrderTool.ViewModels
{
    public partial class OrderEstimationViewModel : ViewModelBase
    {
        // リポジトリ・サービス
        private readonly OrderRepository _orderRepo;
        private readonly PlayerRepository _playerRepo;
        private readonly AliasRepository _aliasRepo;
        private readonly RelationshipExtractor _extractor;
        private readonly OrderSorter _sorter;
        private readonly DbSchemaService _schemaService;

        // --- Bindings (画面と同期するプロパティ) ---

        // 入力欄
        [ObservableProperty]
        private string _inputText = "";

        // ステータスメッセージ
        [ObservableProperty]
        private string _statusText = "準備完了";

        // 環境名 (TEST / PROD)
        [ObservableProperty]
        private string _envText = "";

        // 環境バッジの色
        [ObservableProperty]
        private IBrush _envBadgeColor = Brushes.Gray;

        // ランキングリスト
        public ObservableCollection<string> RankingList { get; } = new();

        public OrderEstimationViewModel()
        {
            // 初期化
            _orderRepo = new OrderRepository();
            _playerRepo = new PlayerRepository();
            _aliasRepo = new AliasRepository();
            _extractor = new RelationshipExtractor();
            _sorter = new OrderSorter();
            _schemaService = new DbSchemaService();

            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // テーブル作成 & 環境表示更新
                _schemaService.EnsureTablesExist();
                UpdateEnvironmentDisplay();
                LoadOrder();
            }
            catch (Exception ex)
            {
                StatusText = $"❌ 初期化エラー: {ex.Message}";
            }
        }

        private void UpdateEnvironmentDisplay()
        {
            string envName = _orderRepo.GetEnvironmentName();
            EnvText = envName;
            EnvBadgeColor = (envName == "PROD") ? Brushes.DarkRed : Brushes.Green;
        }

        // --- Commands (ボタン処理) ---

        // [RelayCommand]をつけると、自動的に「RegisterCommand」という名前でBindingできるようになります
        [RelayCommand]
        private void Register()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                StatusText = "⚠️ 入力が空です";
                return;
            }

            try
            {
                // 1. 正規化
                var aliasDict = _aliasRepo.GetAliasDictionary();
                string normalizedInput = _extractor.NormalizeInput(InputText, aliasDict);

                // 2. 登録プロセス
                _orderRepo.AddObservation(normalizedInput);

                var pairs = _extractor.ExtractFromInput(normalizedInput);
                if (pairs.Count == 0)
                {
                    StatusText = "⚠️ 有効なペアが見つかりませんでした";
                    return;
                }

                var playerNames = pairs.Select(p => p.Predecessor)
                                       .Concat(pairs.Select(p => p.Successor))
                                       .Distinct();

                _playerRepo.RegisterPlayers(playerNames);
                _orderRepo.UpdatePairs(pairs);

                // メッセージ更新
                if (InputText != normalizedInput)
                    StatusText = $"✅ 登録完了 (変換あり): \n'{InputText}' \n→ '{normalizedInput}'";
                else
                    StatusText = $"✅ 登録完了: {pairs.Count} 件の関係を更新しました";

                InputText = ""; // 入力欄クリア
                LoadOrder();    // リスト更新
            }
            catch (Exception ex)
            {
                StatusText = $"❌ エラー: {ex.Message}";
            }
        }

        [RelayCommand]
        private void Reload()
        {
            LoadOrder();
        }

        // ダイアログ表示はViewの責務とするため、このメソッドは「削除実行」のみを担当
        public void PerformClear()
        {
            try
            {
                _orderRepo.ClearAllData();
                _playerRepo.ClearAll();
                _aliasRepo.ClearAll();

                StatusText = "🗑️ データを全削除しました";
                LoadOrder();
            }
            catch (Exception ex)
            {
                StatusText = $"❌ 削除エラー: {ex.Message}";
            }
        }

        private void LoadOrder()
        {
            try
            {
                var allPairs = _orderRepo.GetAllPairs();
                var sortedLayers = _sorter.Sort(allPairs);

                RankingList.Clear();
                int currentRank = 1;

                foreach (var group in sortedLayers)
                {
                    string line = (group.Count == 1)
                        ? $"{currentRank} : {group[0]}"
                        : $"{currentRank} : {string.Join(", ", group)} (推定同列)";

                    RankingList.Add(line);
                    currentRank++;
                }
            }
            catch (Exception ex)
            {
                StatusText = $"❌ 読み込みエラー: {ex.Message}";
            }
        }
    }
}