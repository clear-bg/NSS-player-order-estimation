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

    public virtual async Task AddAliasAsync(string alias, string target)
    {
      // 1. 重複チェック (MySQLのエラーコード判定ではなく、アプリ側で事前にチェックする)
      // これにより SQLite など他のDBでも同じ挙動になります。
      var exists = await _context.Aliases
                                 .AnyAsync(a => a.AliasName == alias);

      if (exists)
      {
        throw new InvalidOperationException($"エイリアス '{alias}' は既に登録されています。");
      }

      // 2. 追加
      var entity = new AliasEntity
      {
        AliasName = alias,
        TargetPlayerId = target
      };

      _context.Aliases.Add(entity);
      await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAliasAsync(string alias)
    {
      // 条件に合うものを一括削除 (通常は1件)
      await _context.Aliases
          .Where(a => a.AliasName == alias)
          .ExecuteDeleteAsync();
    }

    public virtual async Task<Dictionary<string, string>> GetAliasDictionaryAsync()
    {
      // 全件取得して辞書化
      return await _context.Aliases
          .ToDictionaryAsync(
              a => a.AliasName,
              a => a.TargetPlayerId,
              StringComparer.OrdinalIgnoreCase // 大文字小文字を区別しない設定
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