using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NssOrderTool.ViewModels.Arena
{
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
    public IBrush CellColor => WinningTeam switch
    {
      1 => Brushes.CornflowerBlue, // Blue
      2 => Brushes.SandyBrown,     // Orange
      _ => Brushes.LightGray       // Gray
    };

    // 文字色
    public IBrush ForeColor => WinningTeam == 0 ? Brushes.Black : Brushes.White;

    // クリック時のトグルコマンド
    [RelayCommand]
    public void ToggleWinner()
    {
      WinningTeam = (WinningTeam + 1) % 3;
    }
  }
}
