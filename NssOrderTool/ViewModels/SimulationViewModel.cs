using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain;
using NssOrderTool.Messages;

namespace NssOrderTool.ViewModels
{
    public partial class SimulationViewModel : ViewModelBase
    {
        private readonly OrderRepository _orderRepo;
        private readonly AliasRepository _aliasRepo;
        private readonly OrderSorter _sorter;
        private readonly RelationshipExtractor _extractor;

        // --- Bindings ---

        // 8人分の入力フォームデータ
        public ObservableCollection<SimulationInputItem> Inputs { get; } = new();

        // 計算結果の表示リスト
        public ObservableCollection<SimulationResultItem> SimulationResults { get; } = new();

        // 全プレイヤー名リスト (オートコンプリート用)
        public ObservableCollection<string> AllPlayerNames { get; } = new();

        [ObservableProperty]
        private string _statusText = "";

        public SimulationViewModel(
            OrderRepository orderRepo,
            AliasRepository aliasRepo,
            OrderSorter sorter,
            RelationshipExtractor extractor)
        {
            _orderRepo = orderRepo;
            _aliasRepo = aliasRepo;
            _sorter = sorter;
            _extractor = extractor;

            InitializeInputs();

            // DB更新通知を受け取ったらリストをリロードする
            WeakReferenceMessenger.Default.Register<SimulationViewModel, DatabaseUpdatedMessage>(this, (r, m) =>
            {
                // r は this (SimulationViewModelインスタンス) です
                // 非同期メソッドをファイア＆フォーゲットで呼び出します
                _ = r.LoadPlayerNames();
            });
        }

        // デザイナー用コンストラクタ
        public SimulationViewModel()
        {
            _orderRepo = null!;
            _aliasRepo = null!;
            _sorter = null!;
            _extractor = null!;
            InitializeInputs();
        }

        private void InitializeInputs()
        {
            // 8つの入力枠を初期化
            for (int i = 0; i < 8; i++)
            {
                var item = new SimulationInputItem { Index = i + 1 };

                // 1番目はホストとしてプレースホルダーを特別扱い
                if (i == 0)
                    item.Placeholder = "👑 部屋主 (Host)";
                else
                    item.Placeholder = $"Player {i + 1}";

                Inputs.Add(item);
            }
          _ = LoadPlayerNames();
        }

        private async Task LoadPlayerNames()
        {
            try
            {
                // 全ての順序データを取得し、登場する名前(Predecessor, Successor)を重複なしで抽出
                var pairs = await _orderRepo.GetAllPairsAsync();

                var names = pairs.SelectMany(p => new[] { p.Predecessor, p.Successor })
                                 .Distinct()
                                 .OrderBy(n => n)
                                 .ToList();

                AllPlayerNames.Clear();
                foreach (var name in names)
                {
                    AllPlayerNames.Add(name);
                }
            }
            catch (System.Exception ex)
            {
                // 補完リストのロード失敗はメイン動作に影響しないため、ログ出力程度か無視でも可
                // 必要であれば StatusText に出す
                System.Diagnostics.Debug.WriteLine($"Error loading player names: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task RunSimulation()
        {
            if (IsBusy) return;
            IsBusy = true;
            SimulationResults.Clear();
            StatusText = "計算中...";

            try
            {
                // 1. 全データの順序関係を取得して計算 (全体ランキング作成)
                var allPairs = await _orderRepo.GetAllPairsAsync();
                var globalLayers = _sorter.Sort(allPairs);

                // 計算高速化のため、名前 -> ランク(階層ID) の辞書を作成
                // ランクは数字が小さいほど上 (0, 1, 2...)
                var rankMap = new Dictionary<string, int>();
                for (int i = 0; i < globalLayers.Count; i++)
                {
                    foreach (var name in globalLayers[i])
                    {
                        rankMap[name] = i; // 同じ階層なら同じランク値
                    }
                }

                // 2. 入力値の取得と正規化
                var aliasDict = await _aliasRepo.GetAliasDictionaryAsync();
                var participants = new List<Participant>();

                // 入力欄をループ
                for (int i = 0; i < Inputs.Count; i++)
                {
                    var rawName = Inputs[i].Name?.Trim();
                    if (string.IsNullOrWhiteSpace(rawName)) continue;

                    // エイリアス変換 (例: Taka -> Takahiro)
                    // NormalizeInputはカンマ区切り用なので、ここでは単一名変換ロジックを簡易的に使用
                    string normalized = rawName;
                    if (aliasDict.TryGetValue(rawName, out string? target))
                    {
                        normalized = target;
                    }

                    // 参加者リストに追加
                    participants.Add(new Participant
                    {
                        OriginalIndex = i,      // 入力欄の位置 (0ならホスト)
                        InputName = rawName,
                        NormalizedName = normalized,
                        // ランク取得 (データがない場合は int.MaxValue で最下位扱い)
                        GlobalRank = rankMap.ContainsKey(normalized) ? rankMap[normalized] : int.MaxValue
                    });
                }

                if (!participants.Any())
                {
                    StatusText = "⚠️ プレイヤー名を入力してください";
                    return;
                }

                // 3. 今回の部屋内でのソート実行
                var sortedParticipants = participants.OrderBy(p =>
                {
                    // ルール1: ホスト(1行目)は絶対に一番上
                    if (p.OriginalIndex == 0) return int.MinValue;

                    // ルール2: DBの推定ランク順
                    return p.GlobalRank;
                })
                .ThenBy(p => p.OriginalIndex) // 同率なら入力順
                .ToList();

                // 4. 結果表示
                for (int i = 0; i < sortedParticipants.Count; i++)
                {
                    var p = sortedParticipants[i];
                    var item = new SimulationResultItem { PlayerName = p.InputName };

                    // 順位決定ロジック
                    if (i == 0)
                    {
                        // 1人目は必ず1位
                        item.Rank = 1;
                    }
                    else
                    {
                        var prevP = sortedParticipants[i - 1];

                        // 直前の人とランク値が同じなら「同順位」とする
                        // ※ただし、直前の人がホスト(OriginalIndex==0)の場合は、ホストは特例なので同順位にしない
                        if (prevP.OriginalIndex != 0 && p.GlobalRank == prevP.GlobalRank)
                        {
                            // 同率処理: ランクは前の人と同じ
                            item.Rank = SimulationResults[i - 1].Rank;
                            item.IsTied = true;

                            // 前の人も「同率」フラグを立てる
                            SimulationResults[i - 1].IsTied = true;
                        }
                        else
                        {
                            // 通常処理: 現在のインデックス + 1 (1, 2, 2, 4... の形式)
                            item.Rank = i + 1;
                        }
                    }

                    // 付加情報の構築
                    if (p.OriginalIndex == 0)
                    {
                        item.IsHost = true;
                        item.Suffix = " (👑 固定)";
                    }
                    else if (p.GlobalRank == int.MaxValue)
                    {
                        item.Suffix = " (❓ データなし)";
                    }
                    else if (p.InputName != p.NormalizedName)
                    {
                        item.Suffix = $" (← {p.NormalizedName})";
                    }

                    SimulationResults.Add(item);
                }

                // 5. 同率グループの色分け処理
                AssignTiedGroupColors();

                StatusText = "✅ シミュレーション完了";
            }
            catch (System.Exception ex)
            {
                StatusText = $"❌ エラー: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AssignTiedGroupColors()
        {
            // 同率(IsTied=true)のアイテムを、ランクごとにグループ化
            var tiedGroups = SimulationResults
                .Where(r => r.IsTied)
                .GroupBy(r => r.Rank)
                .OrderBy(g => g.Key) // ランク上位(数字が小さい順)
                .ToList();

            // 指定のカラーパレット (スカイブルー, 黄緑, 青)
            var colors = new[]
            {
                SolidColorBrush.Parse("#00BFFF"), // 1組目: DeepSkyBlue (視認性のため少し濃いめ)
                SolidColorBrush.Parse("#9ACD32"), // 2組目: YellowGreen
                SolidColorBrush.Parse("#0000FF")  // 3組目: Blue
            };

            int groupIndex = 0;
            foreach (var group in tiedGroups)
            {
                // 4組目以降は黒(デフォルト)のまま
                if (groupIndex < colors.Length)
                {
                    var color = colors[groupIndex];
                    foreach (var item in group)
                    {
                        item.RankColor = color;
                    }
                    groupIndex++;
                }
            }
        }

        [RelayCommand]
        private void ClearInputs()
        {
            foreach (var item in Inputs)
            {
                item.Name = "";
            }
            SimulationResults.Clear();
            StatusText = "";
        }

        // 内部計算用クラス
        private class Participant
        {
            public int OriginalIndex { get; set; }
            public string InputName { get; set; } = "";
            public string NormalizedName { get; set; } = "";
            public int GlobalRank { get; set; }
        }
    }

    public partial class SimulationResultItem : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RankText))]
        private int _rank;

        [ObservableProperty]
        private string _playerName = "";

        [ObservableProperty]
        private string _suffix = "";

        [ObservableProperty]
        private bool _isHost;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RankWeight))]
        private bool _isTied;

        // RankColorを計算ではなく、直接セット可能なプロパティに変更
        [ObservableProperty]
        private IBrush _rankColor = Brushes.Black;

        public string RankText => $"{Rank}.";

        // 色指定ロジックは削除し、プロパティ (_rankColor) を直接使う

        public FontWeight RankWeight => IsTied ? FontWeight.Bold : FontWeight.Normal;
    }

    // 入力欄1行分のデータクラス
    public partial class SimulationInputItem : ObservableObject
    {
        public int Index { get; set; }
        public string Placeholder { get; set; } = "";

        [ObservableProperty]
        private string _name = "";
    }
}