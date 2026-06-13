using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Database;
using NssOrderTool.Models.Entities; // Entityを利用

namespace NssOrderTool.Repositories
{
  public class AliasRepository
  {
    private readonly AppDbContext _context;

    public AliasRepository(AppDbContext context)
    {
      _context = context;
    }

    public virtual async Task AddAliasAsync(string alias, string targetName)
    {
      // 1. 重複チェック (MySQLのエラーコード判定ではなく、アプリ側で事前にチェックする)
      // これにより SQLite など他のDBでも同じ挙動になります。
      var exists = await _context.Aliases.AnyAsync(a => a.AliasName == alias);
      if (exists)
      {
        throw new InvalidOperationException($"エイリアス '{alias}' は既に登録されています。");
      }

      var targetPlayer = await _context.Players.FirstOrDefaultAsync(p => p.Name == targetName);
      if (targetPlayer == null)
      {
        throw new InvalidOperationException($"対象プレイヤー '{targetName}' が見つかりません。");
      }

      // 2. 追加
      var entity = new AliasEntity
      {
        AliasName = alias,
        TargetPlayerId = targetPlayer.Id
      };

      _context.Aliases.Add(entity);
      await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAliasAsync(string alias)
    {
      // 条件に合うものを一括削除 (通常は1件) (論理削除)
      await _context.Aliases
          .Where(a => a.AliasName == alias)
          .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDeleted, true));
    }

    public virtual async Task<Dictionary<string, string>> GetAliasDictionaryAsync()
    {
      var aliases = await _context.Aliases
          .Select(a => new
          {
            a.AliasName,
            TargetName = _context.Players.Where(p => p.Id == a.TargetPlayerId).Select(p => p.Name).FirstOrDefault()
          })
          .ToListAsync();

      return aliases.ToDictionary(
          a => a.AliasName,
          a => a.TargetName ?? "Unknown",
          StringComparer.OrdinalIgnoreCase
      );
    }

    public virtual async Task<List<string>> GetAliasesByTargetAsync(string targetName)
    {
      return await _context.Aliases
          .Where(a => a.TargetPlayerId == targetName)
          .OrderBy(a => a.AliasName)
          .Select(a => a.AliasName)
          .ToListAsync();
    }

    public virtual async Task ClearAllAsync()
    {
      await _context.Aliases.ExecuteDeleteAsync();
    }

    public virtual void ResetTracking()
    {
      _context.ChangeTracker.Clear();
    }
  }
}
