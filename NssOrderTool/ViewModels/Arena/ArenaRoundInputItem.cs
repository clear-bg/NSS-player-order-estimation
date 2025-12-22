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
    [NotifyPropertyChangedFor(nameof(HoverColor))]
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
      1 => Brush.Parse("#9BC2E6"),
      2 => Brush.Parse("#F8CBAD"),
      _ => Brush.Parse("#f0f0f0"),
    };


    public IBrush HoverColor => WinningTeam switch
    {
      // CornflowerBlue(#6495ED) より少し明るい青
      1 => Brush.Parse("#82B1FF"),

      // SandyBrown(#F4A460) より少し明るいオレンジ
      2 => Brush.Parse("#FFCC80"),

      // LightGray(#D3D3D3) より少し明るいグレー
      _ => Brush.Parse("#E0E0E0")
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
