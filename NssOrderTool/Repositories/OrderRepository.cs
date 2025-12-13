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

            // 1. 入力リスト内で同じペアを集約する。
            var inputCounts = pairs
                .GroupBy(p => new { p.Predecessor, p.Successor })
                .Select(g => new
                {
                    g.Key.Predecessor,
                    g.Key.Successor,
                    Count = g.Count()
                })
                .ToList();

            // 2. 検索効率化のため、対象のIDリストを作成
            var preds = inputCounts.Select(x => x.Predecessor).Distinct().ToList();
            var succs = inputCounts.Select(x => x.Successor).Distinct().ToList();

            // 3. 該当しそうな既存データをまとめて取得
            var existingEntities = await _context.SequencePairs
                .Where(p => preds.Contains(p.PredecessorId) && succs.Contains(p.SuccessorId))
                .ToListAsync();

            // 4. メモリ上で照合して処理
            foreach (var item in inputCounts)
            {
                var entity = existingEntities.FirstOrDefault(e =>
                    e.PredecessorId == item.Predecessor &&
                    e.SuccessorId == item.Successor);

                if (entity != null)
                {
                    // 既存ならカウントアップ（集計した分を足す）
                    entity.Frequency += item.Count;
                }
                else
                {
                    // 新規なら追加（Frequencyは入力された回数分）
                    _context.SequencePairs.Add(new SequencePairEntity
                    {
                        PredecessorId = item.Predecessor,
                        SuccessorId = item.Successor,
                        Frequency = item.Count
                    });
                }
            }

            // 5. 一括保存
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

        public virtual async Task MergePlayerIdsAsync(string oldName, string newName)
        {
            // 大文字小文字の違いだけであれば統合不要（同一IDとみなされるため）
            // ただし、DB照合順序によっては区別される場合もあるので、念のためIDが完全に一致しない場合のみ実行
            if (string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase)) return;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // --- 1. SequencePairs (順序データ) の統合 ---

                // A. oldName が Predecessor (勝ち側) の場合
                var predOldPairs = await _context.SequencePairs
                    .Where(p => p.PredecessorId == oldName)
                    .ToListAsync();

                foreach (var oldPair in predOldPairs)
                {
                    // 移行先 (newName -> sameSuccessor) があるか確認
                    var targetPair = await _context.SequencePairs
                        .FirstOrDefaultAsync(p => p.PredecessorId == newName && p.SuccessorId == oldPair.SuccessorId);

                    if (targetPair != null)
                    {
                        // 既に存在する場合はカウントを加算して、古い方を削除
                        targetPair.Frequency += oldPair.Frequency;
                        _context.SequencePairs.Remove(oldPair);
                    }
                    else
                    {
                        // 存在しない場合は、新しいキーで作り直して追加し、古い方を削除
                        _context.SequencePairs.Add(new SequencePairEntity
                        {
                            PredecessorId = newName,
                            SuccessorId = oldPair.SuccessorId,
                            Frequency = oldPair.Frequency
                        });
                        _context.SequencePairs.Remove(oldPair);
                    }
                }

                // B. oldName が Successor (負け側) の場合
                var succOldPairs = await _context.SequencePairs
                    .Where(p => p.SuccessorId == oldName)
                    .ToListAsync();

                foreach (var oldPair in succOldPairs)
                {
                    var targetPair = await _context.SequencePairs
                        .FirstOrDefaultAsync(p => p.PredecessorId == oldPair.PredecessorId && p.SuccessorId == newName);

                    if (targetPair != null)
                    {
                        targetPair.Frequency += oldPair.Frequency;
                        _context.SequencePairs.Remove(oldPair);
                    }
                    else
                    {
                        _context.SequencePairs.Add(new SequencePairEntity
                        {
                            PredecessorId = oldPair.PredecessorId,
                            SuccessorId = newName,
                            Frequency = oldPair.Frequency
                        });
                        _context.SequencePairs.Remove(oldPair);
                    }
                }

                // --- 2. Players (プレイヤーマスタ) の統合 ---
                var oldPlayer = await _context.Players.FindAsync(oldName);
                var newPlayer = await _context.Players.FindAsync(newName);

                if (oldPlayer != null)
                {
                    if (newPlayer != null)
                    {
                        // 両方いる場合: 古い方のFirstSeenが古ければ採用し、古いレコードは消す
                        if (oldPlayer.FirstSeen < newPlayer.FirstSeen)
                        {
                            newPlayer.FirstSeen = oldPlayer.FirstSeen;
                        }
                        _context.Players.Remove(oldPlayer);
                    }
                    else
                    {
                        // 新しい方がまだいない場合: IDを書き換えた新しいレコードを作成
                        _context.Players.Add(new PlayerEntity
                        {
                            Id = newName,
                            Name = newName,
                            FirstSeen = oldPlayer.FirstSeen
                        });
                        _context.Players.Remove(oldPlayer);
                    }
                }

                // --- 3. Observations (履歴ログ) の置換 ---
                // ログ内の文字列も置換する
                var observations = await _context.Observations
                    .Where(o => o.OrderedList.Contains(oldName))
                    .ToListAsync();

                foreach (var obs in observations)
                {
                    // カンマ区切りで分解して正確に置換
                    var names = obs.OrderedList.Split(',')
                        .Select(n => n.Trim())
                        .Select(n => n.Equals(oldName, StringComparison.OrdinalIgnoreCase) ? newName : n);

                    obs.OrderedList = string.Join(", ", names);
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