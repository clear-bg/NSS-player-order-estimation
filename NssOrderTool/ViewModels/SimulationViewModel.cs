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
using NssOrderTool.Models.UI;

namespace NssOrderTool.ViewModels
{
  public partial class SimulationViewModel : ViewModelBase
  {
    private readonly OrderRepository _orderRepo;
    private readonly AliasRepository _aliasRepo;
    private readonly OrderSorter _sorter;

    // --- Bindings ---

    public ObservableCollection<SimulationInputItem> Inputs { get; } = new();
    public ObservableCollection<SimulationResultItem> SimulationResults { get; } = new();
    public ObservableCollection<string> AllPlayerNames { get; } = new();

    [ObservableProperty]
    private string _statusText = "";

    public SimulationViewModel(
        OrderRepository orderRepo,
        AliasRepository aliasRepo,
        OrderSorter sorter)
    {
      _orderRepo = orderRepo;
      _aliasRepo = aliasRepo;
      _sorter = sorter;

      InitializeInputs();

      WeakReferenceMessenger.Default.Register<SimulationViewModel, DatabaseUpdatedMessage>(this, (r, m) =>
      {
        _ = r.LoadPlayerNames();
      });
    }

    public SimulationViewModel()
    {
      _orderRepo = null!;
      _aliasRepo = null!;
      _sorter = null!;
      InitializeInputs();
    }

    private void InitializeInputs()
    {
      for (int i = 0; i < 8; i++)
      {
        var item = new SimulationInputItem { Index = i + 1 };
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
        // 1. 全データの順序関係を取得して階層化
        var allPairs = await _orderRepo.GetAllPairsAsync();
        var globalLayers = _sorter.Sort(allPairs);

        // 名前 -> 配置順(階層ID) の辞書を作成
        // 数字が小さいほど上に配置される (0, 1, 2...)
        var orderMap = new Dictionary<string, int>();
        for (int i = 0; i < globalLayers.Count; i++)
        {
          foreach (var name in globalLayers[i])
          {
            orderMap[name] = i;
          }
        }

        // 2. 入力値の取得と正規化
        var aliasDict = await _aliasRepo.GetAliasDictionaryAsync();
        var participants = new List<Participant>();

        for (int i = 0; i < Inputs.Count; i++)
        {
          var rawName = Inputs[i].Name?.Trim();
          if (string.IsNullOrWhiteSpace(rawName)) continue;

          string normalized = rawName;
          if (aliasDict.TryGetValue(rawName, out string? target))
          {
            normalized = target;
          }

          participants.Add(new Participant
          {
            OriginalIndex = i,
            InputName = rawName,
            NormalizedName = normalized,
            // 配置順取得 (データがない場合は int.MaxValue で最下位扱い)
            GlobalOrder = orderMap.ContainsKey(normalized) ? orderMap[normalized] : int.MaxValue
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

          // ルール2: DBの推定配置順
          return p.GlobalOrder;
        })
        .ThenBy(p => p.OriginalIndex) // 同率なら入力順
        .ToList();

        // 4. 結果表示
        for (int i = 0; i < sortedParticipants.Count; i++)
        {
          var p = sortedParticipants[i];
          var item = new SimulationResultItem { PlayerName = p.InputName };

          // 配置順（インデックス）決定ロジック
          if (i == 0)
          {
            item.OrderIndex = 1;
          }
          else
          {
            var prevP = sortedParticipants[i - 1];

            // 直前の人と階層値が同じなら「同配置順」とする
            if (prevP.OriginalIndex != 0 && p.GlobalOrder == prevP.GlobalOrder)
            {
              item.OrderIndex = SimulationResults[i - 1].OrderIndex;
              item.IsTied = true;
              SimulationResults[i - 1].IsTied = true;
            }
            else
            {
              item.OrderIndex = i + 1;
            }
          }

          if (p.OriginalIndex == 0)
          {
            item.IsHost = true;
            item.Suffix = " (👑 固定)";
          }
          else if (p.GlobalOrder == int.MaxValue)
          {
            item.Suffix = " (❓ データなし)";
          }
          else if (p.InputName != p.NormalizedName)
          {
            item.Suffix = $" (← {p.NormalizedName})";
          }

          SimulationResults.Add(item);
        }

        // 5. 同配置順グループの色分け処理
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
      var tiedGroups = SimulationResults
          .Where(r => r.IsTied)
          .GroupBy(r => r.OrderIndex)
          .OrderBy(g => g.Key)
          .ToList();

      var colors = new[]
      {
          SolidColorBrush.Parse("#00BFFF"),
          SolidColorBrush.Parse("#9ACD32"),
          SolidColorBrush.Parse("#0000FF")
      };

      int groupIndex = 0;
      foreach (var group in tiedGroups)
      {
        foreach (var item in group)
        {
          item.TiedGroupIndex = groupIndex;
        }
        groupIndex++;
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

    private class Participant
    {
      public int OriginalIndex { get; set; }
      public string InputName { get; set; } = "";
      public string NormalizedName { get; set; } = "";
      public int GlobalOrder { get; set; }
    }

    [RelayCommand]
    private void TransferToArena()
    {
      if (SimulationResults.Count == 0) return;
      var names = SimulationResults.Select(x => x.PlayerName).ToList();
      WeakReferenceMessenger.Default.Send(new TransferToArenaMessage(names));
    }
  }

  public partial class SimulationResultItem : ObservableObject
  {
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrderText))]
    private int _orderIndex;

    [ObservableProperty]
    private string _playerName = "";

    [ObservableProperty]
    private string _suffix = "";

    [ObservableProperty]
    private bool _isHost;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrderWeight))]
    private bool _isTied;

    [ObservableProperty]
    private int _tiedGroupIndex = -1; // 初期値の -1 は「同配置順（タイ）ではない」ことを表します

    public string OrderText => $"{OrderIndex}.";

    public FontWeight OrderWeight => IsTied ? FontWeight.Bold : FontWeight.Normal;
  }
}
