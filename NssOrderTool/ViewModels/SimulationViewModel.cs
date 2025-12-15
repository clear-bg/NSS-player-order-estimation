using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain;

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
        public ObservableCollection<string> SimulationResults { get; } = new();

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
                int displayRank = 1;
                foreach (var p in sortedParticipants)
                {
                    string suffix = "";

                    if (p.OriginalIndex == 0)
                    {
                        suffix = " (👑 固定)";
                    }
                    else if (p.GlobalRank == int.MaxValue)
                    {
                        suffix = " (❓ データなし)";
                    }
                    // エイリアス変換があった場合のみ表示
                    else if (p.InputName != p.NormalizedName)
                    {
                        suffix = $" (← {p.NormalizedName})";
                    }

                    SimulationResults.Add($"{displayRank}. {p.InputName}{suffix}");
                    displayRank++;
                }

                StatusText = "✅ シミュレーション完了";
            }
            finally
            {
                IsBusy = false;
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

    // 入力欄1行分のデータクラス
    public partial class SimulationInputItem : ObservableObject
    {
        public int Index { get; set; }
        public string Placeholder { get; set; } = "";

        [ObservableProperty]
        private string _name = "";
    }
}