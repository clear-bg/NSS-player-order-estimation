using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Database;
using NssOrderTool.Models.Entities;

namespace NssOrderTool.Repositories
{
  public class SeasonRepository
  {
    private readonly AppDbContext _context;

    public SeasonRepository(AppDbContext context)
    {
      _context = context;
    }

    // 全シーズンの取得（開始日時の降順などでソートしておくと便利です）
    public async Task<List<SeasonEntity>> GetAllSeasonsAsync()
    {
      return await _context.Seasons
          .OrderByDescending(s => s.StartDate)
          .ToListAsync();
    }

    // 現在アクティブなシーズンを取得
    public async Task<SeasonEntity?> GetActiveSeasonAsync()
    {
      return await _context.Seasons
          .FirstOrDefaultAsync(s => s.IsActive);
    }

    // シーズンの追加
    public async Task AddSeasonAsync(SeasonEntity season)
    {
      _context.Seasons.Add(season);
      await _context.SaveChangesAsync();
    }

    // シーズンの更新
    public async Task UpdateSeasonAsync(SeasonEntity season)
    {
      _context.Seasons.Update(season);
      await _context.SaveChangesAsync();
    }

    // シーズンの削除（※必要に応じて）
    public async Task DeleteSeasonAsync(SeasonEntity season)
    {
      _context.Seasons.Remove(season);
      await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 開始日を 00:00:00 に正規化する
    /// </summary>
    public DateTime NormalizeStartDate(DateTime date)
    {
      return date.Date; // 時刻が切り捨てられ、00:00:00 になります
    }

    /// <summary>
    /// 終了日を 23:59:59 に正規化する
    /// </summary>
    public DateTime NormalizeEndDate(DateTime date)
    {
      return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
    }

    /// <summary>
    /// 指定されたシーズンが、他のシーズンと期間重複していないか判定する
    /// </summary>
    public async Task<bool> IsSeasonOverlappingAsync(SeasonEntity targetSeason)
    {
      // ターゲットの終了日が未定(null)の場合は、未来永劫(MaxValue)として扱う
      var targetStart = targetSeason.StartDate;
      var targetEnd = targetSeason.EndDate ?? DateTime.MaxValue;

      // 編集時のことを考慮し、DBから「自分自身以外の」全シーズンを取得
      var otherSeasons = await _context.Seasons
          .Where(s => s.Id != targetSeason.Id)
          .ToListAsync();

      foreach (var s in otherSeasons)
      {
        var otherStart = s.StartDate;
        var otherEnd = s.EndDate ?? DateTime.MaxValue;

        // 【重複の判定条件】
        // 対象Aの開始日が、比較先Bの終了日以前である
        // かつ
        // 対象Aの終了日が、比較先Bの開始日以降である
        if (targetStart <= otherEnd && targetEnd >= otherStart)
        {
          return true; // 重複あり
        }
      }

      return false; // 重複なし（安全）
    }
  }
}
