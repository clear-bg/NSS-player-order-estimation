using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Database;
using NssOrderTool.Models;
using NssOrderTool.Repositories;
using Xunit;

namespace NssOrderTool.Tests.Repositories
{
    public class OrderRepositoryTests
    {
        private AppDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task UpdatePairsAsync_ShouldIncrementFrequency_WhenPairExists()
        {
            // Arrange (準備)
            var dbName = "TestDb_UpdatePairs"; // テストごとにユニークな名前推奨

            // 1. 初期データを投入するコンテキスト
            using (var context = CreateInMemoryContext(dbName))
            {
                var repo = new OrderRepository(context, new AppConfig());
                // 初回のペア登録 (A -> B)
                var pairs = new List<OrderPair> { new OrderPair("A", "B") };
                await repo.UpdatePairsAsync(pairs);
            }

            // Act (実行)
            // 2. 同じペアをもう一度登録するコンテキスト (アプリ再起動後のイメージ)
            using (var context = CreateInMemoryContext(dbName))
            {
                var repo = new OrderRepository(context, new AppConfig());
                var pairs = new List<OrderPair> { new OrderPair("A", "B") };

                // 実行: ここで Frequency が 2 になるはず
                await repo.UpdatePairsAsync(pairs);
            }

            // Assert (検証)
            // 3. 結果を確認するコンテキスト
            using (var context = CreateInMemoryContext(dbName))
            {
                var entity = await context.SequencePairs.FirstOrDefaultAsync();

                entity.Should().NotBeNull();
                entity!.PredecessorId.Should().Be("A");
                entity.SuccessorId.Should().Be("B");
                entity.Frequency.Should().Be(2, "同じペアが2回登録されたため");
            }
        }

        [Fact]
        public async Task UpdatePairsAsync_ShouldInsertNew_WhenPairDoesNotExist()
        {
            // Arrange
            var dbName = "TestDb_InsertNew";

            using (var context = CreateInMemoryContext(dbName))
            {
                var repo = new OrderRepository(context, new AppConfig());
                var pairs = new List<OrderPair>
                {
                    new OrderPair("X", "Y"),
                    new OrderPair("Y", "Z")
                };

                // Act
                await repo.UpdatePairsAsync(pairs);
            }

            // Assert
            using (var context = CreateInMemoryContext(dbName))
            {
                var count = await context.SequencePairs.CountAsync();
                count.Should().Be(2);

                var xy = await context.SequencePairs
                    .FirstAsync(p => p.PredecessorId == "X" && p.SuccessorId == "Y");
                xy.Frequency.Should().Be(1);
            }
        }
    }
}