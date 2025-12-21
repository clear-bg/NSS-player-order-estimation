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
      1 => Brushes.CornflowerBlue,
      2 => Brushes.SandyBrown,
      _ => Brushes.LightGray
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
