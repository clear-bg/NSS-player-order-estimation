using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;

namespace NssOrderTool.ViewModels
{
  public partial class ArenaViewModel : ViewModelBase
  {
    private readonly ArenaRepository _arenaRepo;

    // --- Bindings ---

    // 14ラウンド分の入力データ
    public ObservableCollection<ArenaRoundInputItem> RoundInputs { get; } = new();

    [ObservableProperty]
    private string _statusText = "準備完了";

    // コンストラクタ
    public ArenaViewModel(ArenaRepository arenaRepo)
    {
      _arenaRepo = arenaRepo;
      InitializeRounds();
    }

    // XAMLデザイナー用
    public ArenaViewModel()
    {
      _arenaRepo = null!;
      InitializeRounds();
    }

    private void InitializeRounds()
    {
      RoundInputs.Clear();
      for (int i = 1; i <= 14; i++)
      {
        RoundInputs.Add(new ArenaRoundInputItem { RoundNumber = i });
      }
    }

    [RelayCommand]
    private async Task SaveSession()
    {
      if (IsBusy) return;
      IsBusy = true;
      StatusText = "保存中...";

      try
      {
        // 1. セッション作成 (一旦プレイヤー名は空。Phase 3で連携)
        var session = new ArenaSessionEntity
        {
          PlayerIdsCsv = "",
          CreatedAt = DateTime.Now
        };

        // 2. ラウンド結果の変換
        foreach (var input in RoundInputs)
        {
          session.Rounds.Add(new ArenaRoundEntity
          {
            RoundNumber = input.RoundNumber,
            WinningTeam = input.WinningTeam // 0=None, 1=Blue, 2=Orange
          });
        }

        // 3. 保存
        await _arenaRepo.AddSessionAsync(session);

        StatusText = "✅ 結果を保存しました";
        InitializeRounds(); // 入力をリセット
      }
      catch (Exception ex)
      {
        StatusText = $"❌ エラー: {ex.Message}";
      }
      finally
      {
        IsBusy = false;
      }
    }
  }

  // 各ラウンドのセル（ボタン）に対応するクラス
  public partial class ArenaRoundInputItem : ObservableObject
  {
    public int RoundNumber { get; set; }

    // 0: 未選択, 1: Blue, 2: Orange
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    [NotifyPropertyChangedFor(nameof(CellColor))]
    [NotifyPropertyChangedFor(nameof(ForeColor))]
    private int _winningTeam = 0;

    // 表示テキスト (Excel風にシンプルに)
    public string DisplayText => WinningTeam switch
    {
      1 => "Blue",
      2 => "Org",
      _ => "-"
    };

    // 背景色 (Excelに近い色味)
    public string CellColor => WinningTeam switch
    {
      1 => "#4472C4", // Excel標準の青
      2 => "#ED7D31", // Excel標準のオレンジ
      _ => "#F2F2F2"  // グレー
    };

    // 文字色
    public string ForeColor => WinningTeam == 0 ? "Black" : "White";

    // クリック時のトグルコマンド
    [RelayCommand]
    public void ToggleWinner()
    {
      WinningTeam = (WinningTeam + 1) % 3;
    }
  }
}
