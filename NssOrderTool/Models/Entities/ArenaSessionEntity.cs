using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Models.Entities
{
  [Table("ArenaSessions")]
  public class ArenaSessionEntity : ISoftDelete, ITimestamp
  {
    [Key]
    [Column("session_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("session_date")]
    public DateTime SessionDate { get; set; }

    // --- Navigation Properties ---

    // 参加者リスト
    public List<ArenaParticipantEntity> Participants { get; set; } = new();

    // リレーション (1対多)
    public List<ArenaRoundEntity> Rounds { get; set; } = new();

    public string PlayersJson { get; set; } = "[]";

    [NotMapped]
    public string WinningTeam
    {
      get
      {
        if (Rounds == null || Rounds.Count == 0) return "-";

        // 修正: 文字列 "Blue" ではなく 数値 1 で比較
        var blueWins = Rounds.Count(r => r.WinningTeam == 1);

        // 修正: 文字列 "Orange" ではなく 数値 2 で比較
        var orangeWins = Rounds.Count(r => r.WinningTeam == 2);

        if (blueWins > orangeWins) return "Blue";
        if (orangeWins > blueWins) return "Orange";
        return "Draw";
      }
    }

    [NotMapped]
    public string HostName
    {
      get
      {
        if (string.IsNullOrWhiteSpace(PlayersJson))
          return "Unknown";

        try
        {
          // JSON文字列 ["Aさん", "Bさん", ...] をリストに復元
          var players = JsonSerializer.Deserialize<List<string>>(PlayersJson);

          // 1人目を返す（いなければ Unknown）
          return players?.FirstOrDefault() ?? "Unknown";
        }
        catch
        {
          return "Error";
        }
      }
    }

    [NotMapped]
    public string HistorySummaryText
    {
      get
      {
        if (string.IsNullOrWhiteSpace(PlayersJson))
          return "データなし";

        try
        {
          // JSON文字列からリストを復元
          var players = JsonSerializer.Deserialize<List<string>>(PlayersJson);

          if (players == null || players.Count == 0)
            return "データなし";

          // 1人目は必ずホスト
          string host = players[0];

          // 2人目以降をランキングとして処理（最大3位まで）
          var ranks = new List<string>();
          for (int i = 1; i < players.Count && i <= 3; i++)
          {
            ranks.Add($"{i}位: {players[i]}");
          }

          // ランキングがない場合はホスト名のみ、ある場合は結合して返す
          if (ranks.Count == 0)
            return $"ホスト: {host}";

          return $"ホスト: {host} | {string.Join(", ", ranks)}";
        }
        catch
        {
          return "データ読み込みエラー";
        }
      }
    }
  }
}
