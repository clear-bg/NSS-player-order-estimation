using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Avalonia.Media;
using NssOrderTool.Messages;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain;

namespace NssOrderTool.ViewModels
{
  public partial class SimulationViewModel : ViewModelBase
  {
    private readonly OrderRepository _orderRepo;
    private readonly AliasRepository _aliasRepo;
    private readonly PlayerRepository _playerRepo;
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

    [ObservableProperty]
    private IBrush _statusTextColor = Brushes.Green;

    public SimulationViewModel(
        OrderRepository orderRepo,
        AliasRepository aliasRepo,
        PlayerRepository playerRepo,
        OrderSorter sorter,
        RelationshipExtractor extractor)
    {
      _orderRepo = orderRepo;
      _aliasRepo = aliasRepo;
      _playerRepo = playerRepo;
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
      _playerRepo = null!;
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
      StatusText = "計算中...";
      StatusTextColor = Brushes.Black;
      SimulationResults.Clear();

      var inputNames = Inputs
          .Where(x => !string.IsNullOrWhiteSpace(x.Name))
          .Select(x => x.Name.Trim())
          .ToList();

      if (inputNames.Count < 2)
      {
        StatusText = "❌ エラー: 2人以上入力してください。";
        StatusTextColor = Brushes.Red;
        return;
      }

      var aliasDict = await _aliasRepo.GetAliasDictionaryAsync();
      var allPlayers = await _playerRepo.GetAllPlayersAsync();

      var inputUuids = new List<string>();
      var uuidToNameMap = new Dictionary<string, string>();

      foreach (var name in inputNames)
      {
        string targetName = name;
        // エイリアス辞書に登録があれば、UUID経由で本名を特定
        if (aliasDict.TryGetValue(name, out string? targetId))
        {
          var aliasPlayer = allPlayers.FirstOrDefault(p => p.Id == targetId);
          if (aliasPlayer != null) targetName = aliasPlayer.Name;
        }

        // 本名からUUIDを取得
        var player = allPlayers.FirstOrDefault(p => p.Name == targetName && !p.IsDeleted);
        if (player != null)
        {
          inputUuids.Add(player.Id);
          uuidToNameMap[player.Id] = player.Name; // 出口のために「UUID = 名前」を記憶しておく
        }
      }

      // DBから既存の全ペア（UUID）を取得し、今回の入力の隣接ペア（UUID）を合流させる
      var allPairs = await _orderRepo.GetAllPairsAsync();
      var currentInputPairs = _extractor.ExtractPairs(inputUuids);
      allPairs.AddRange(currentInputPairs);

      // SorterにUUIDのグラフを渡してソート実行
      var sortedUuidLayers = _sorter.Sort(allPairs);

      int currentRank = 1;
      foreach (var layer in sortedUuidLayers)
      {
        // この順位レイヤーに含まれるUUIDのうち、今回画面に入力された人だけを抽出
        var targetUuidsInLayer = layer.Where(uuid => inputUuids.Contains(uuid)).ToList();

        if (targetUuidsInLayer.Any())
        {
          bool isTied = targetUuidsInLayer.Count > 1; // 同着判定
          foreach (var uuid in targetUuidsInLayer)
          {
            // 記憶しておいた辞書から本名を復元
            if (uuidToNameMap.TryGetValue(uuid, out string? playerName))
            {
              SimulationResults.Add(new SimulationResultItem
              {
                Rank = currentRank,
                PlayerName = playerName,
                IsTied = isTied,
                IsHost = (playerName == inputNames.FirstOrDefault()) // 先頭の人をホスト扱い
              });
            }
          }
          // 同着人数分だけ次の順位を飛ばす（例: 1位が2人なら次は3位）
          currentRank += targetUuidsInLayer.Count;
        }
      }

      StatusText = "✅ 計算完了";
      StatusTextColor = Brushes.Green;
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

    [RelayCommand]
    private void TransferToArena()
    {
      if (SimulationResults.Count == 0) return;

      if (SimulationResults.Count < 8)
      {
        StatusText = "❌ エラー: アリーナ集計へ反映するには、8人全員の入力が必要です。";
        StatusTextColor = Brushes.Red;
        return;
      }

      // 結果リストからプレイヤー名だけを抽出してリスト化
      // ※ SimulationResultItem.PlayerName プロパティを使用
      var names = SimulationResults.Select(x => x.PlayerName).ToList();

      // メッセージ送信
      WeakReferenceMessenger.Default.Send(new TransferToArenaMessage(names));

      StatusText = "✅ アリーナ集計へ反映しました。";
      StatusTextColor = Brushes.Green;
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
