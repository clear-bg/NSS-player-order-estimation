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
  }
}
