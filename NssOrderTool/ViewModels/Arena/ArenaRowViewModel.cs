using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using NssOrderTool.Services.Domain;

namespace NssOrderTool.ViewModels.Arena
{
  public partial class ArenaRowViewModel : ViewModelBase
  {
    public int Index { get; }
    [ObservableProperty]
    private string _name;

    // 14ラウンド分のセル
    public ObservableCollection<ArenaCellViewModel> Cells { get; } = new();

    [ObservableProperty]
    private int _winCount;

    [ObservableProperty]
    private int _rank;

    // 将来的に相性を復活させる場合用（今はプレースホルダー）
    [ObservableProperty]
    private string _compatibilityText = "-";

    public ArenaRowViewModel(int index, string name)
    {
      Index = index;
      Name = name;

      for (int i = 1; i <= 14; i++)
      {
        Cells.Add(new ArenaCellViewModel(i));
      }
    }

    /// <summary>
    /// この行の全セルのチームと勝敗を更新する
    /// </summary>
    /// <param name="roundInputs">全ラウンドの勝敗入力状況</param>
    /// <param name="logic">チーム判定ロジック</param>
    public void UpdateRow(ObservableCollection<ArenaRoundInputItem> roundInputs, ArenaLogicService logic)
    {
      int currentWins = 0;

      for (int i = 0; i < 14; i++)
      {
        var cell = Cells[i];
        int roundNumber = i + 1;

        // 1. チーム判定 (Domain Serviceを利用)
        int myTeam = logic.GetTeamId(roundNumber, Index);
        cell.TeamId = myTeam;

        // 2. 勝敗更新
        int winner = roundInputs[i].WinningTeam;
        cell.UpdateState(winner);

        if (cell.IsWinner)
        {
          currentWins++;
        }
      }

      WinCount = currentWins;
    }
  }
}
