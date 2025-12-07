using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

        [ObservableProperty]
        private string _inputText = "";

        [ObservableProperty]
        private string _statusText = "準備完了";

        [ObservableProperty]
        private string _envText = "";

        [ObservableProperty]
        private IBrush _envBadgeColor = Brushes.Gray;

        public ObservableCollection<string> RankingList { get; } = new();

        public OrderEstimationViewModel(
            OrderRepository orderRepo,
            PlayerRepository playerRepo,
            AliasRepository aliasRepo,
            RelationshipExtractor extractor,
            OrderSorter sorter,
            DbSchemaService schemaService)
        {
            _orderRepo = orderRepo;
            _playerRepo = playerRepo;
            _aliasRepo = aliasRepo;
            _extractor = extractor;
            _sorter = sorter;
            _schemaService = schemaService;

            InitializeAsync();
        }

        // デザイナー用の空コンストラクタ（あるとVSのプレビューが動く）
        public OrderEstimationViewModel()
        {
            /* デザイン時はnullのままで落ちるかもしれないが、一旦許容 */
            _orderRepo = null!;
            _playerRepo = null!;
            _aliasRepo = null!;
            _extractor = null!;
            _sorter = null!;
            _schemaService = null!;
        }

        private async void InitializeAsync()
        {
            try
            {
                // テーブル作成 (ここは同期メソッドのままだが、高速なので許容)
                _schemaService.EnsureTablesExist();

                UpdateEnvironmentDisplay();

                // 初回読み込み (非同期)
                await LoadOrderAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"❌ 初期化エラー: {ex.Message}";
            }
        }

        private void UpdateEnvironmentDisplay()
        {
            // 環境名はメモリ上の設定を読むだけなので同期でOK
            string envName = _orderRepo.GetEnvironmentName();
            EnvText = envName;
            EnvBadgeColor = (envName == "PROD") ? Brushes.DarkRed : Brushes.Green;
        }

        // --- Commands (ボタン処理) ---

        [RelayCommand]
        private async Task Register()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                StatusText = "⚠️ 入力が空です";
                return;
            }

            try
            {
                // 1. エイリアス辞書を非同期で取得して正規化
                var aliasDict = await _aliasRepo.GetAliasDictionaryAsync();
                string normalizedInput = _extractor.NormalizeInput(InputText, aliasDict);

                // 2. 観測ログ保存
                await _orderRepo.AddObservationAsync(normalizedInput);

                // 3. ペア分解
                var pairs = _extractor.ExtractFromInput(normalizedInput);
                if (pairs.Count == 0)
                {
                    StatusText = "⚠️ 有効なペアが見つかりませんでした";
                    return;
                }

                // 4. プレイヤー登録 & 関係更新
                var playerNames = pairs.Select(p => p.Predecessor)
                                       .Concat(pairs.Select(p => p.Successor))
                                       .Distinct();

                await _playerRepo.RegisterPlayersAsync(playerNames);
                await _orderRepo.UpdatePairsAsync(pairs);

                // メッセージ更新
                if (InputText != normalizedInput)
                    StatusText = $"✅ 登録完了 (変換あり): \n'{InputText}' \n→ '{normalizedInput}'";
                else
                    StatusText = $"✅ 登録完了: {pairs.Count} 件の関係を更新しました";

                InputText = ""; // 入力欄クリア

                // リスト再読み込み
                await LoadOrderAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"❌ エラー: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task Reload()
        {
            await LoadOrderAsync();
        }

        // Viewのコードビハインドから呼ばれるメソッド (全削除)
        public async Task PerformClearAsync()
        {
            try
            {
                await _orderRepo.ClearAllDataAsync();
                await _playerRepo.ClearAllAsync();
                await _aliasRepo.ClearAllAsync();

                StatusText = "🗑️ データを全削除しました";
                await LoadOrderAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"❌ 削除エラー: {ex.Message}";
            }
        }

        private async Task LoadOrderAsync()
        {
            try
            {
                // DBから全ペアを非同期取得
                var allPairs = await _orderRepo.GetAllPairsAsync();

                // ソート計算 (オンメモリ処理)
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