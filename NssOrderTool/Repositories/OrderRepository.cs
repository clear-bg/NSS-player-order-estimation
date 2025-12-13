using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Database;
using NssOrderTool.Models;
using NssOrderTool.Models.Entities; // Entityを使う

namespace NssOrderTool.Repositories
{
    public class OrderRepository
    {
        private readonly AppDbContext _context;
        private readonly AppConfig _config; // 環境名取得用

        // DbManagerの代わりに AppDbContext と AppConfig を受け取る
        public OrderRepository(AppDbContext context, AppConfig config)
        {
            _context = context;
            _config = config;
        }

        public virtual async Task AddObservationAsync(string rawInput)
        {
            var entity = new ObservationEntity
            {
                OrderedList = rawInput,
                ObservationTime = DateTime.Now
            };

            // SQL: INSERT INTO Observations ...
            _context.Observations.Add(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdatePairsAsync(List<OrderPair> pairs)
        {
            if (pairs == null || !pairs.Any()) return;

            // --- Upsertロジック (EF Core版) ---
            // SQLの ON DUPLICATE KEY UPDATE を使わず、
            // 「読み込んでチェックして、あれば更新/なければ追加」を行います。

            // 1. 検索効率化のため、対象のIDリストを作成
            var preds = pairs.Select(p => p.Predecessor).Distinct().ToList();
            var succs = pairs.Select(p => p.Successor).Distinct().ToList();

            // 2. 該当しそうな既存データをまとめて取得
            var existingEntities = await _context.SequencePairs
                .Where(p => preds.Contains(p.PredecessorId) && succs.Contains(p.SuccessorId))
                .ToListAsync();

            // 3. メモリ上で照合して処理
            foreach (var pair in pairs)
            {
                var entity = existingEntities.FirstOrDefault(e =>
                    e.PredecessorId == pair.Predecessor &&
                    e.SuccessorId == pair.Successor);

                if (entity != null)
                {
                    // 既存ならカウントアップ
                    entity.Frequency++;
                }
                else
                {
                    // 新規なら追加
                    _context.SequencePairs.Add(new SequencePairEntity
                    {
                        PredecessorId = pair.Predecessor,
                        SuccessorId = pair.Successor,
                        Frequency = 1
                    });
                }
            }

            // 4. 一括保存
            await _context.SaveChangesAsync();
        }

        public virtual async Task<List<OrderPair>> GetAllPairsAsync()
        {
            return await _context.SequencePairs
                .Select(p => new OrderPair(p.PredecessorId, p.SuccessorId))
                .ToListAsync();
        }

        public virtual async Task ClearAllDataAsync()
        {
            // 一括削除
            await _context.SequencePairs.ExecuteDeleteAsync();
            await _context.Observations.ExecuteDeleteAsync();
        }

        public virtual string GetEnvironmentName()
        {
            // DbManager経由ではなく、設定クラスから直接取得
            return _config.AppSettings?.Environment ?? "UNKNOWN";
        }

        public virtual async Task<List<ObservationEntity>> GetRecentObservationsAsync(int limit = 50)
        {
            return await _context.Observations
                .OrderByDescending(o => o.ObservationTime)
                .Take(limit)
                .ToListAsync();
        }

        public virtual async Task UndoObservationAsync(int observationId, List<OrderPair> pairsToDecrement)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. ペアのFrequencyを減算 (存在チェック含む)
                if (pairsToDecrement != null && pairsToDecrement.Any())
                {
                    var preds = pairsToDecrement.Select(p => p.Predecessor).Distinct().ToList();
                    var succs = pairsToDecrement.Select(p => p.Successor).Distinct().ToList();

                    var entities = await _context.SequencePairs
                        .Where(p => preds.Contains(p.PredecessorId) && succs.Contains(p.SuccessorId))
                        .ToListAsync();

                    foreach (var pair in pairsToDecrement)
                    {
                        var entity = entities.FirstOrDefault(e =>
                            e.PredecessorId == pair.Predecessor &&
                            e.SuccessorId == pair.Successor);

                        if (entity != null)
                        {
                            entity.Frequency--;
                            // 0以下になったらレコード削除 (ゴミ掃除)
                            if (entity.Frequency <= 0)
                            {
                                _context.SequencePairs.Remove(entity);
                            }
                        }
                    }
                }

                // 2. 履歴ログ(Observation)を削除
                var obs = await _context.Observations.FindAsync(observationId);
                if (obs != null)
                {
                    _context.Observations.Remove(obs);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public virtual void ResetTracking()
        {
            _context.ChangeTracker.Clear();
        }
    }
}