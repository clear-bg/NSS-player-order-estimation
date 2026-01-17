using System;
using System.Collections.Generic;

namespace NssOrderTool.Models
{
  // B-1, B-2: 基本戦績データ
  public class PlayerStatsDto
  {
    public int TotalSessions { get; set; }
    public int SessionWins { get; set; }
    public double SessionWinRate { get; set; }
    public double AverageRank { get; set; }

    public int TotalRounds { get; set; }

    public int BlueRounds { get; set; }
    public int BlueWins { get; set; }
    public double BlueWinRate { get; set; }

    public int OrangeRounds { get; set; }
    public int OrangeWins { get; set; }
    public double OrangeWinRate { get; set; }
  }

  // B-3: 試合履歴データ
  public class MatchHistoryDto
  {
    public DateTime Date { get; set; }

    // ★修正: 初期値 = "" を代入
    public string Result { get; set; } = "";

    public int MyRank { get; set; }
    public int WinCount { get; set; }
    public string PartnerName { get; set; } = "";
  }

  // B-4: 相性データ
  public class SynergyDto
  {
    public string PlayerName { get; set; } = "";
    public int RoundCount { get; set; }
    public int WinCount { get; set; }
    public double WinRate { get; set; }
  }

  // 全体まとめクラス
  public class PlayerDetailsDto
  {
    public PlayerStatsDto Stats { get; set; } = new();
    public List<MatchHistoryDto> History { get; set; } = new();
    public List<SynergyDto> BestPartners { get; set; } = new();

    // ★修正: 初期値 = new() を代入
    public List<SynergyDto> WorstRivals { get; set; } = new();
  }
}
