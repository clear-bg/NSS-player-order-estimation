using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NssOrderTool.Database;
using NssOrderTool.Models;
using NssOrderTool.Models.Entities;

namespace NssOrderTool.Repositories
{
  public class ArenaRepository
  {
    private readonly AppDbContext _context;
    private readonly IServiceProvider _services;

    public ArenaRepository(AppDbContext context, IServiceProvider services)
    {
      _context = context;
      _services = services;
    }

    // --- 既存メソッド (変更なし) ---
    public virtual async Task AddSessionAsync(ArenaSessionEntity session)
    {
      _context.ArenaSessions.Add(session);
      await _context.SaveChangesAsync();
    }

    public virtual async Task<List<ArenaSessionEntity>> GetAllSessionsAsync()
    {
      return await _context.ArenaSessions
          .Include(s => s.Rounds)
          .Include(s => s.Participants)
          .OrderByDescending(s => s.CreatedAt)
          .ToListAsync();
    }

    public virtual async Task<ArenaSessionEntity?> GetSessionDetailAsync(int sessionId)
    {
      return await _context.ArenaSessions
          .Include(s => s.Rounds)
          .Include(s => s.Participants)
          .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public virtual async Task DeleteSessionAsync(int sessionId)
    {
      var session = await _context.ArenaSessions
          .Include(s => s.Participants)
          .Include(s => s.Rounds)
          .FirstOrDefaultAsync(s => s.Id == sessionId);

      if (session != null)
      {
        session.IsDeleted = true;
        foreach (var p in session.Participants) p.IsDeleted = true;
        foreach (var r in session.Rounds) r.IsDeleted = true;
        await _context.SaveChangesAsync();
      }
    }

    // --- 詳細データ集計ロジック ---

    private static readonly Dictionary<int, int[]> BlueTeamDefinitions = new()
        {
            { 1,  new[] { 0, 1, 2, 3 } }, { 2,  new[] { 0, 2, 4, 6 } }, { 3,  new[] { 0, 3, 4, 7 } },
            { 4,  new[] { 0, 1, 6, 7 } }, { 5,  new[] { 0, 2, 5, 7 } }, { 6,  new[] { 0, 1, 4, 5 } },
            { 7,  new[] { 0, 3, 5, 6 } }, { 8,  new[] { 0, 1, 2, 4 } }, { 9,  new[] { 0, 3, 4, 6 } },
            { 10, new[] { 0, 1, 3, 7 } }, { 11, new[] { 0, 2, 3, 5 } }, { 12, new[] { 0, 2, 6, 7 } },
            { 13, new[] { 0, 1, 5, 6 } }, { 14, new[] { 0, 4, 5, 7 } }
        };

    private bool IsBlueTeam(int roundNumber, int slotIndex)
    {
      if (!BlueTeamDefinitions.ContainsKey(roundNumber)) return false;
      return BlueTeamDefinitions[roundNumber].Contains(slotIndex);
    }

    public async Task<PlayerDetailsDto> GetPlayerDetailsAsync(string playerId)
    {
      using var scope = _services.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

      // 1. データ取得
      // ★修正: Include(p => p.Session!) とすることで、後続のThenIncludeに「nullじゃないよ」と伝えます
      var myParticipations = await context.ArenaParticipants
          .Include(p => p.Session!)
              .ThenInclude(s => s.Rounds)
          .Include(p => p.Session!)
              .ThenInclude(s => s.Participants)
                  .ThenInclude(ap => ap.Player)
          .Where(p => p.PlayerId == playerId && !p.IsDeleted)
          .OrderByDescending(p => p.Session!.CreatedAt)
          .ToListAsync();

      var result = new PlayerDetailsDto();
      if (!myParticipations.Any()) return result;

      // --- 集計処理 ---
      var stats = new PlayerStatsDto();
      stats.TotalSessions = myParticipations.Count;
      stats.SessionWins = myParticipations.Count(p => p.Rank == 1);
      stats.SessionWinRate = (double)stats.SessionWins / stats.TotalSessions;
      stats.AverageRank = myParticipations.Average(p => p.Rank);

      int blueRounds = 0, blueWins = 0;
      int orangeRounds = 0, orangeWins = 0;

      var partnerStats = new Dictionary<string, (int rounds, int wins)>();
      var rivalStats = new Dictionary<string, (int rounds, int wins)>();

      foreach (var p in myParticipations)
      {
        var session = p.Session;
        if (session == null || session.Rounds == null) continue;

        // ★修正: Value側にも ?? "" をつけて、辞書の型を Dictionary<int, string> (非null) に確定させます
        var otherPlayers = session.Participants
            .Where(x => x.PlayerId != playerId && x.Player != null)
            .ToDictionary(x => x.SlotIndex, x => x.Player!.Name ?? "Unknown");

        foreach (var round in session.Rounds)
        {
          bool amIBlue = IsBlueTeam(round.RoundNumber, p.SlotIndex);
          int myTeamId = amIBlue ? 1 : 2;

          bool isWin = (round.WinningTeam == myTeamId);

          if (amIBlue) { blueRounds++; if (isWin) blueWins++; }
          else { orangeRounds++; if (isWin) orangeWins++; }

          foreach (var other in otherPlayers)
          {
            // 辞書のValueが string (非null) になったので警告は出ないはずです
            string otherName = other.Value;
            int otherSlot = other.Key;

            bool isOtherBlue = IsBlueTeam(round.RoundNumber, otherSlot);
            bool isSameTeam = (amIBlue == isOtherBlue);

            if (isSameTeam)
            {
              if (!partnerStats.ContainsKey(otherName)) partnerStats[otherName] = (0, 0);
              var cur = partnerStats[otherName];
              partnerStats[otherName] = (cur.rounds + 1, cur.wins + (isWin ? 1 : 0));
            }
            else
            {
              if (!rivalStats.ContainsKey(otherName)) rivalStats[otherName] = (0, 0);
              var cur = rivalStats[otherName];
              rivalStats[otherName] = (cur.rounds + 1, cur.wins + (isWin ? 1 : 0));
            }
          }
        }
      }

      stats.TotalRounds = blueRounds + orangeRounds;
      stats.BlueRounds = blueRounds;
      stats.BlueWins = blueWins;
      stats.BlueWinRate = blueRounds > 0 ? (double)blueWins / blueRounds : 0;
      stats.OrangeRounds = orangeRounds;
      stats.OrangeWins = orangeWins;
      stats.OrangeWinRate = orangeRounds > 0 ? (double)orangeWins / orangeRounds : 0;

      result.Stats = stats;

      // 履歴リスト
      result.History = myParticipations
                .Where(p => p.Session != null)
                .Take(10)
                .Select(p => new MatchHistoryDto
                {
                  Date = p.Session!.CreatedAt,
                  Result = p.Rank == 1 ? "🏆 1st" : $"{p.Rank}th",
                  MyRank = p.Rank,
                  WinCount = p.WinCount,
                  PartnerName = $"Host: {p.Session.Participants.FirstOrDefault(x => x.SlotIndex == 0)?.Player?.Name ?? "-"}"
                }).ToList();

      // 相性データ
      result.BestPartners = partnerStats
          .Where(x => x.Value.rounds >= 5)
          .Select(x => new SynergyDto
          {
            PlayerName = x.Key,
            RoundCount = x.Value.rounds,
            WinCount = x.Value.wins,
            WinRate = (double)x.Value.wins / x.Value.rounds
          })
          .OrderByDescending(x => x.WinRate).ThenByDescending(x => x.RoundCount)
          .Take(3).ToList();

      result.WorstRivals = rivalStats
          .Where(x => x.Value.rounds >= 5)
          .Select(x => new SynergyDto
          {
            PlayerName = x.Key,
            RoundCount = x.Value.rounds,
            WinCount = x.Value.wins,
            WinRate = (double)x.Value.wins / x.Value.rounds
          })
          .OrderBy(x => x.WinRate).ThenByDescending(x => x.RoundCount)
          .Take(3).ToList();

      return result;
    }
  }
}
