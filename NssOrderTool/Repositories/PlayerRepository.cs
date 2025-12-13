using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Database;
using NssOrderTool.Models.Entities; // Entityを使う

namespace NssOrderTool.Repositories
{
    public class PlayerRepository
    {
        // DbManager ではなく AppDbContext を使う
        private readonly AppDbContext _context;

        public PlayerRepository(AppDbContext context)
        {
            _context = context;
        }

        public virtual async Task RegisterPlayersAsync(IEnumerable<string> players)
        {
            // 重複排除
            var uniqueNames = players.Distinct().ToList();
            if (!uniqueNames.Any()) return;

            // 1. 既にDBに存在する名前を取得
            var existingNames = await _context.Players
                .Where(p => uniqueNames.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            // 2. DBにない名前だけを抽出 (LINQで差分を取る)
            var newNames = uniqueNames.Except(existingNames).ToList();

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

        public async Task<List<string>> GetAllPlayersAsync()
        {
            // SQL: SELECT player_id FROM Players ORDER BY player_id
            return await _context.Players
                .OrderBy(p => p.Id)
                .Select(p => p.Id)
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
    }
}