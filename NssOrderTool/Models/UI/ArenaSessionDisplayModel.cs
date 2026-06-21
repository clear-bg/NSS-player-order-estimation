using System;
using System.Collections.Generic;
using System.Linq;
using NssOrderTool.Models.Entities;

namespace NssOrderTool.Models.UI
{
  // 画面の「履歴リスト」に表示するための専用モデル
  public class ArenaSessionDisplayModel
  {
    public int Id { get; }
    public DateTime SessionDate { get; }
    public string WinningTeam { get; }
    public string HostName { get; }
    public string HistorySummaryText { get; }
    public string Memo { get; set; } = string.Empty;

    // コンストラクタ：Entityを受け取って、表示用の文字列をここで組み立てる
    public ArenaSessionDisplayModel(ArenaSessionEntity entity)
    {
      Id = entity.Id;
      SessionDate = entity.SessionDate;
      Memo = entity.Memo ?? string.Empty;

      // 1. 勝敗ロジック
      int blueWins = entity.Rounds?.Count(r => r.WinningTeam == 1) ?? 0;
      int orangeWins = entity.Rounds?.Count(r => r.WinningTeam == 2) ?? 0;
      if (blueWins > orangeWins) WinningTeam = "Blue";
      else if (orangeWins > blueWins) WinningTeam = "Orange";
      else WinningTeam = "Draw";

      // 2. ホスト名ロジック
      var host = entity.Participants?.OrderBy(p => p.SlotIndex).FirstOrDefault();
      HostName = host?.Player?.Name ?? "Unknown";

      // 3. サマリーテキストロジック
      if (entity.Participants == null || !entity.Participants.Any())
      {
        HistorySummaryText = "データなし";
      }
      else
      {
        var topPlayers = entity.Participants
            .Where(p => p.Rank > 0)
            .OrderBy(p => p.Rank)
            .Select(p => p.Player?.Name ?? "Unknown")
            .Take(3)
            .ToList();

        var ranks = new List<string>();
        for (int i = 0; i < topPlayers.Count; i++)
        {
          ranks.Add($"{i + 1}位: {topPlayers[i]}");
        }

        HistorySummaryText = ranks.Count == 0
            ? $"ホスト: {HostName}"
            : $"ホスト: {HostName} | {string.Join(", ", ranks)}";
      }
    }
  }
}
