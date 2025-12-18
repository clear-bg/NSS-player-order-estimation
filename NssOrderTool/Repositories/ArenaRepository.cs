using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Database;
using NssOrderTool.Models.Entities;

namespace NssOrderTool.Repositories
{
  public class ArenaRepository
  {
    private readonly AppDbContext _context;

    public ArenaRepository(AppDbContext context)
    {
      _context = context;
    }

    /// <summary>
    /// 新しいアリーナセッション（とラウンド結果）を保存する
    /// </summary>
    public virtual async Task AddSessionAsync(ArenaSessionEntity session)
    {
      _context.ArenaSessions.Add(session);
      await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 全てのアリーナセッションを日付の新しい順に取得する（ラウンド情報は含まない）
    /// </summary>
    public virtual async Task<List<ArenaSessionEntity>> GetAllSessionsAsync()
    {
      return await _context.ArenaSessions
          .OrderByDescending(s => s.CreatedAt)
          .ToListAsync();
    }

    /// <summary>
    /// 指定したIDのセッションを、ラウンド情報込みで取得する
    /// </summary>
    public virtual async Task<ArenaSessionEntity?> GetSessionDetailAsync(int sessionId)
    {
      return await _context.ArenaSessions
          .Include(s => s.Rounds) // ラウンド情報も結合して取得
          .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    /// <summary>
    /// 指定したセッションを削除する
    /// </summary>
    public virtual async Task DeleteSessionAsync(int sessionId)
    {
      // カスケード削除の設定によるが、念のため明示的に取得して削除
      var session = await _context.ArenaSessions.FindAsync(sessionId);
      if (session != null)
      {
        _context.ArenaSessions.Remove(session);
        await _context.SaveChangesAsync();
      }
    }

    public virtual void ResetTracking()
    {
      _context.ChangeTracker.Clear();
    }
  }
}
