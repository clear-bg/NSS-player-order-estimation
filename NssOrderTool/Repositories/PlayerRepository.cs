using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Database;
using NssOrderTool.Models.Entities;

namespace NssOrderTool.Repositories
{
  public class PlayerRepository
  {
    private readonly AppDbContext _context;

    public PlayerRepository(AppDbContext context)
    {
      _context = context;
    }

    /// <summary>
    /// プレイヤー名を登録する（存在しない場合のみ新規作成）
    /// テストでモック化可能にするため virtual を付与
    /// </summary>
    public virtual async Task RegisterPlayersAsync(IEnumerable<string> players)
    {
      // 重複排除
      var uniqueNames = players.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
      if (!uniqueNames.Any()) return;

      // 1. 既にDBに存在する名前を取得
      var existingNames = await _context.Players
          .Where(p => uniqueNames.Contains(p.Id))
          .Select(p => p.Id)
          .ToListAsync();

      // 2. DBにない名前だけを抽出 (LINQで差分を取る)
      var newNames = uniqueNames.Except(existingNames, StringComparer.OrdinalIgnoreCase).ToList();

      if (newNames.Any())
      {
        // 3. 新しいプレイヤーを作成して追加
        var newEntities = newNames.Select(name => new PlayerEntity
        {
          Id = name,
          Name = name,
          // FirstSeen はデフォルトで現在時刻が入る
        });

        await _context.Players.AddRangeAsync(newEntities);
        await _context.SaveChangesAsync();
      }
    }

    /// <summary>
    /// 全プレイヤー情報を取得する
    /// 将来の拡張性のため、IDだけでなく Entity 全体を返すように変更
    /// </summary>
    public virtual async Task<List<PlayerEntity>> GetAllPlayersAsync()
    {
      return await _context.Players
          .OrderBy(p => p.Id)
          .ToListAsync();
    }

    public virtual async Task ClearAllAsync()
    {
      // EF Core 7+ の新機能: ExecuteDeleteAsync (高速に全削除)
      await _context.Players.ExecuteDeleteAsync();
    }

    public virtual void ResetTracking()
    {
      _context.ChangeTracker.Clear();
    }

    public virtual async Task<PlayerEntity?> GetPlayerAsync(string id)
    {
      if (string.IsNullOrWhiteSpace(id)) return null;

      return await _context.Players
          .AsNoTracking() // 読み取り専用なので高速化
          .FirstOrDefaultAsync(p => p.Id == id);
    }

    public virtual async Task<List<PlayerEntity>> GetTopRatedPlayersAsync(int count)
    {
      return await _context.Players
          .AsNoTracking()
          .Where(p => !p.IsDeleted) // 削除済みは除外
          .OrderByDescending(p => p.RateMean - (3.0 * p.RateSigma)) // 表示レート順
          .Take(count)
          .ToListAsync();
    }
  }
}
