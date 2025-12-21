using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NssOrderTool.ViewModels.Arena
{
  public partial class ArenaCellViewModel : ViewModelBase
  {
    public int RoundNumber { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CellColor))]
    [NotifyPropertyChangedFor(nameof(ForeColor))]
    private int _teamId; // 1:Blue, 2:Orange

    // ★変更点: 勝った時だけ "1" を表示するためのプロパティ
    [ObservableProperty]
    private string _resultMark = "";

    [ObservableProperty]
    private bool _isWinner;

    // 背景色（チームごとの色）
    public IBrush CellColor => TeamId switch
    {
      1 => Brush.Parse("#9BC2E6"),
      2 => Brush.Parse("#F8CBAD"),
      _ => Brush.Parse("#f0f0f0"),
    };

    // 文字色
    public IBrush ForeColor => Brushes.White;

    public ArenaCellViewModel(int roundNumber)
    {
      RoundNumber = roundNumber;
    }

    /// <summary>
    /// 勝敗状態を更新する
    /// </summary>
    public void UpdateState(int winningTeam)
    {
      if (winningTeam == TeamId)
      {
        ResultMark = "1";
        IsWinner = true;
      }
      else
      {
        ResultMark = "";
        IsWinner = false;
      }
    }
  }
}
