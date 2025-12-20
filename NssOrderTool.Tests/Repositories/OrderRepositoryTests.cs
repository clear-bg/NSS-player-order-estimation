using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NssOrderTool.Database;
using NssOrderTool.Models;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;
using Xunit;

namespace NssOrderTool.Tests.Repositories
{
  public class OrderRepositoryTests
  {
    // In-Memory DB コンテキストを作成するヘルパーメソッド
    private AppDbContext CreateInMemoryContext(string dbName)
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(databaseName: dbName)
          .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
          .Options;

      return new AppDbContext(options);
    }

    [Fact]
    public async Task AddObservationAsync_ShouldSaveObservation_AndDetails()
    {
      // Arrange
      var dbName = "TestDb_AddObservation";
      var input = "PlayerA, PlayerB, PlayerC";
      var config = new AppConfig(); // テスト用の設定（中身は空でOK）

      // Act
      using (var context = CreateInMemoryContext(dbName))
      {
        var repository = new OrderRepository(context, config);
        await repository.AddObservationAsync(input);
      }

      // Assert
      using (var context = CreateInMemoryContext(dbName))
      {
        // 親レコード(Observation)の確認
        var observation = await context.Observations
            .Include(o => o.Details)
            .FirstOrDefaultAsync();

        observation.Should().NotBeNull();
        observation!.Details.Should().HaveCount(3);

        // 子レコード(Details)の順序と内容の確認
        var details = observation.Details.OrderBy(d => d.OrderIndex).ToList();

        details[0].PlayerId.Should().Be("PlayerA");
        details[0].OrderIndex.Should().Be(0);

        details[1].PlayerId.Should().Be("PlayerB");
        details[1].OrderIndex.Should().Be(1);

        details[2].PlayerId.Should().Be("PlayerC");
        details[2].OrderIndex.Should().Be(2);
      }
    }

    [Fact]
    public async Task GetRecentObservationsAsync_ShouldReturnObservations()
    {
      // Arrange
      var dbName = "TestDb_GetRecent";
      var config = new AppConfig();

      using (var context = CreateInMemoryContext(dbName))
      {
        var repository = new OrderRepository(context, config);
        await repository.AddObservationAsync("A, B");
        await repository.AddObservationAsync("C, D");
      }

      // Act
      using (var context = CreateInMemoryContext(dbName))
      {
        var repository = new OrderRepository(context, config);
        var result = await repository.GetRecentObservationsAsync(10);

        // Assert
        result.Should().HaveCount(2);
        // 新しい順に取得されるはず
        result[0].Details.Should().Contain(d => d.PlayerId == "C"); // ※注: 実装にIncludeがない場合、ここでDetailsが取得できない可能性がありますが、テストとしては期待値を記述します
      }
    }

    [Fact]
    public async Task UpdatePairsAsync_ShouldIncrementFrequency_WhenPairExists()
    {
      // Arrange
      var dbName = "TestDb_UpdatePairs";
      var config = new AppConfig();

      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new OrderRepository(context, config);
        var pairs = new List<OrderPair> { new OrderPair("A", "B") };
        await repo.UpdatePairsAsync(pairs);
      }

      // Act: 同じペアを再度追加
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new OrderRepository(context, config);
        var pairs = new List<OrderPair> { new OrderPair("A", "B") };
        await repo.UpdatePairsAsync(pairs);
      }

      // Assert
      using (var context = CreateInMemoryContext(dbName))
      {
        var entity = await context.SequencePairs.FirstOrDefaultAsync();
        entity.Should().NotBeNull();
        entity!.Frequency.Should().Be(2);
      }
    }
  }
}
