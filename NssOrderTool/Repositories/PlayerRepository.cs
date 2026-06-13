using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Database;
using NssOrderTool.Models.Entities;
using NssOrderTool.Services.Rating;

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
    // 既存の RegisterPlayersAsync を以下のメソッドに置き換えてください
    /// <summary>
    /// 名前からUUIDを取得する。allowCreateがtrueなら未登録プレイヤーを新規作成する。
    /// </summary>
    public virtual async Task<Dictionary<string, string>> GetOrCreatePlayersAsync(IEnumerable<string> playerNames, bool allowCreate = false)
    {
      var uniqueNames = playerNames.Where(n => !string.IsNullOrWhiteSpace(n))
                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                   .ToList();
      if (!uniqueNames.Any()) return new Dictionary<string, string>();

      // 1. 「名前」でDBを検索
      var existingPlayers = await _context.Players
          .Where(p => uniqueNames.Contains(p.Name!))
          .ToListAsync();

      var resultMap = existingPlayers.ToDictionary(p => p.Name!, p => p.Id, StringComparer.OrdinalIgnoreCase);

      // 2. 新規作成が許可されている場合 (順序推定ツール側など)
      if (allowCreate)
      {
        var existingNames = existingPlayers.Select(p => p.Name!);
        var newNames = uniqueNames.Except(existingNames, StringComparer.OrdinalIgnoreCase).ToList();

        if (newNames.Any())
        {
          var newEntities = newNames.Select(name => new PlayerEntity
          {
            // IdはGuid.NewGuid()で自動生成される
            Name = name
          }).ToList();

          await _context.Players.AddRangeAsync(newEntities);
          await _context.SaveChangesAsync();

          foreach (var entity in newEntities)
          {
            resultMap[entity.Name!] = entity.Id; // 新しいUUIDを辞書に追加
          }
        }
      }

      return resultMap;
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
          .Where(p => !p.IsDeleted)
          // ★変更: RateMean (単純な高さ) でソート
          .OrderByDescending(p => p.RateMean)
          .Take(count)
          .ToListAsync();
    }

    public virtual async Task UpdatePlayerRatingsAsync(Dictionary<string, NssOrderTool.Services.Rating.RatingData> newRatings)
    {
      if (newRatings == null || newRatings.Count == 0) return;

      var ids = newRatings.Keys.ToList();

      // 対象のプレイヤーをDBから取得
      var players = await _context.Players
          .Where(p => ids.Contains(p.Id))
          .ToListAsync();

      foreach (var player in players)
      {
        if (newRatings.TryGetValue(player.Id, out var rating))
        {
          player.RateMean = rating.Mean;
          // player.RateSigma = rating.Sigma; // Sigmaを使わないならコメントアウトのままでOK
          player.UpdatedAt = DateTime.Now;
        }
      }

      await _context.SaveChangesAsync();
    }
  }
}
