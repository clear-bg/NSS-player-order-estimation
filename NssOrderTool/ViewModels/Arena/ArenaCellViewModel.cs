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

    [ObservableProperty]
    private string _resultMark = ""; // "Win" etc

    [ObservableProperty]
    private bool _isWinner;

    // Excel風の色定義
    public IBrush CellColor => TeamId switch
    {
      1 => Brushes.CornflowerBlue, // Blue
      2 => Brushes.SandyBrown,     // Orange
      _ => Brushes.LightGray
    };

    // 文字色（背景が濃いので白、グレーの時は非表示っぽく薄くなど）
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
      if (winningTeam == 0)
      {
        ResultMark = "";
        IsWinner = false;
      }
      else if (winningTeam == TeamId)
      {
        ResultMark = "Win";
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
