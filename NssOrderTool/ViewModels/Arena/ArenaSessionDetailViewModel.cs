using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using NssOrderTool.Models.Entities;

// ※名前空間はご自身の環境に合わせてください (NssOrderTool.ViewModels.Arena かもしれません)
namespace NssOrderTool.ViewModels
{
  public partial class ArenaSessionDetailViewModel : ViewModelBase
  {
    [ObservableProperty]
    private ArenaSessionEntity _session;

    public ObservableCollection<RoundDetailItem> RoundDetails { get; } = new();

    public ArenaSessionDetailViewModel(ArenaSessionEntity session)
    {
      // 修正: _session ではなく Session を使う
      Session = session;
      LoadDetails();
    }

    private void LoadDetails()
    {
      // 修正: _session ではなく Session を使う
      if (Session?.Rounds == null || Session?.Participants == null) return;

      var rounds = Session.Rounds.OrderBy(r => r.RoundNumber).ToList();
      var participants = Session.Participants.OrderBy(p => p.SlotIndex).ToList();

      foreach (var round in rounds)
      {
        RoundDetails.Add(new RoundDetailItem
        {
          RoundNumber = round.RoundNumber,
          WinningTeam = round.WinningTeam,
          WinnerName = round.WinningTeam == 1 ? "🔵 Blue" : round.WinningTeam == 2 ? "🟠 Orange" : "引き分け"
        });
      }
    }
  }

  public class RoundDetailItem
  {
    public int RoundNumber { get; set; }
    public int WinningTeam { get; set; }
    public string WinnerName { get; set; } = string.Empty;
  }
}
