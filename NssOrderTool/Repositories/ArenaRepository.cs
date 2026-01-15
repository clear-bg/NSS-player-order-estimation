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
          .Include(s => s.Rounds)
          .Include(s => s.Participants)
          .OrderByDescending(s => s.CreatedAt)
          .ToListAsync();
    }

    /// <summary>
    /// 指定したIDのセッションを、ラウンド情報込みで取得する
    /// </summary>
    public virtual async Task<ArenaSessionEntity?> GetSessionDetailAsync(int sessionId)
    {
      return await _context.ArenaSessions
          .Include(s => s.Rounds)
          .Include(s => s.Participants)
          .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    /// <summary>
    /// 指定したセッションを削除する
    /// </summary>
    public virtual async Task DeleteSessionAsync(int sessionId)
    {
      // 1. 関連データ(Participants, Rounds)も含めて取得する
      var session = await _context.ArenaSessions
          .Include(s => s.Participants)
          .Include(s => s.Rounds)
          .FirstOrDefaultAsync(s => s.Id == sessionId);

      if (session != null)
      {
        // 2. 親セッションの論理削除
        session.IsDeleted = true;

        // 3. 子（参加者）の論理削除
        foreach (var p in session.Participants)
        {
          p.IsDeleted = true;
        }

        // 4. 子（ラウンド）の論理削除
        foreach (var r in session.Rounds)
        {
          r.IsDeleted = true;
        }

        // 5. 保存 (UPDATE文が発行される)
        await _context.SaveChangesAsync();
      }
    }

    public virtual void ResetTracking()
    {
      _context.ChangeTracker.Clear();
    }
  }
}
