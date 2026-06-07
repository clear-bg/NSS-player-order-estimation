using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using NssOrderTool.Models.Entities;

namespace NssOrderTool.ViewModels
{
  public partial class ArenaSessionDetailViewModel : ViewModelBase
  {
    [ObservableProperty]
    private ArenaSessionEntity _session;

    // プレイヤー1人につき1行のデータ
    public ObservableCollection<PlayerResultRow> PlayerRows { get; } = new();

    // どのラウンドで誰がブルーチームか（ArenaRepositoryと同じルール）
    private static readonly Dictionary<int, int[]> BlueTeamDefinitions = new()
    {
        { 1,  new[] { 0, 1, 2, 3 } }, { 2,  new[] { 0, 2, 4, 6 } }, { 3,  new[] { 0, 3, 4, 7 } },
        { 4,  new[] { 0, 1, 6, 7 } }, { 5,  new[] { 0, 2, 5, 7 } }, { 6,  new[] { 0, 1, 4, 5 } },
        { 7,  new[] { 0, 3, 5, 6 } }, { 8,  new[] { 0, 1, 2, 4 } }, { 9,  new[] { 0, 3, 4, 6 } },
        { 10, new[] { 0, 1, 3, 7 } }, { 11, new[] { 0, 2, 3, 5 } }, { 12, new[] { 0, 2, 6, 7 } },
        { 13, new[] { 0, 1, 5, 6 } }, { 14, new[] { 0, 4, 5, 7 } }
    };

    public ArenaSessionDetailViewModel(ArenaSessionEntity session)
    {
      Session = session;
      LoadMatrix();
    }

    private void LoadMatrix()
    {
      if (Session?.Participants == null || Session?.Rounds == null) return;

      var participants = Session.Participants.OrderBy(p => p.SlotIndex).ToList();
      var rounds = Session.Rounds.OrderBy(r => r.RoundNumber).ToList();

      foreach (var p in participants)
      {
        var row = new PlayerResultRow
        {
          Name = string.IsNullOrWhiteSpace(p.PlayerId) ? $"Player {p.SlotIndex + 1}" : p.PlayerId,
          WinCount = p.WinCount,
          Rank = p.Rank
        };

        for (int i = 1; i <= 14; i++)
        {
          var roundData = rounds.FirstOrDefault(r => r.RoundNumber == i);
          int winningTeam = roundData?.WinningTeam ?? 0; // 1:Blue, 2:Orange, 0:None

          bool isBlue = BlueTeamDefinitions.ContainsKey(i) && BlueTeamDefinitions[i].Contains(p.SlotIndex);
          int myTeam = isBlue ? 1 : 2;

          // ★変更: 常にチームカラーの背景色を設定（画像に近い淡い青とオレンジ）
          string colorHex = isBlue ? "#99C2E8" : "#F4CA9F";
          string resultText = "";

          // 勝敗がついている場合
          if (winningTeam != 0)
          {
            bool isWin = (myTeam == winningTeam);
            // ★変更: 勝ったら"1"、負けたら空文字
            resultText = isWin ? "1" : "";
          }

          row.RoundResults.Add(new RoundResultItem
          {
            ResultText = resultText,
            BackgroundColor = colorHex,
            TextColor = "#333333" // 文字色は見やすい濃いグレー
          });
        }

        PlayerRows.Add(row);
      }
    }
  }

  // UI表示用のクラス
  public class PlayerResultRow
  {
    public string Name { get; set; } = string.Empty;
    public int WinCount { get; set; }
    public int Rank { get; set; }
    public List<RoundResultItem> RoundResults { get; set; } = new();
  }

  public class RoundResultItem
  {
    public string ResultText { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = "Transparent";
    public string TextColor { get; set; } = "White";
  }
}
