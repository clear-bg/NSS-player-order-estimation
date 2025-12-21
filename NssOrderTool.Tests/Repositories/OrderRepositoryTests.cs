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
          .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // トランザクション無視警告を抑制
          .Options;

      return new AppDbContext(options);
    }

    [Fact]
    public async Task AddObservationAsync_ShouldSaveObservation_AndDetails()
    {
      // Arrange
      var dbName = "TestDb_AddObservation";
      var input = "PlayerA, PlayerB, PlayerC";
      var config = new AppConfig();

      // Act
      using (var context = CreateInMemoryContext(dbName))
      {
        var repository = new OrderRepository(context, config);
        await repository.AddObservationAsync(input);
      }

      // Assert
      using (var context = CreateInMemoryContext(dbName))
      {
        var observation = await context.Observations
            .Include(o => o.Details)
            .FirstOrDefaultAsync();

        observation.Should().NotBeNull();
        observation!.Details.Should().HaveCount(3);

        var details = observation.Details.OrderBy(d => d.OrderIndex).ToList();
        details[0].PlayerId.Should().Be("PlayerA");
      }
    }

    [Fact]
    public async Task AddObservationAsync_ShouldSetAuditColumns()
    {
      // Arrange
      var dbName = "TestDb_AuditColumns";
      var config = new AppConfig();
      var before = DateTime.Now;

      // Act
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new OrderRepository(context, config);
        await repo.AddObservationAsync("A, B");
      }

      // Assert
      using (var context = CreateInMemoryContext(dbName))
      {
        var obs = await context.Observations.FirstAsync();

        // 作成日時・更新日時が入っていること
        obs.CreatedAt.Should().BeAfter(before.AddSeconds(-1));
        obs.CreatedAt.Should().BeBefore(DateTime.Now.AddSeconds(1));
        obs.UpdatedAt.Should().Be(obs.CreatedAt); // 新規作成時は同じはず
      }
    }

    [Fact]
    public async Task DeleteObservationAsync_LogicalDelete_WorksCorrectly()
    {
      // Arrange
      var dbName = "TestDb_LogicalDelete";
      var config = new AppConfig();
      int targetId;

      // データ準備
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new OrderRepository(context, config);
        await repo.AddObservationAsync("DeleteMeA, DeleteMeB");
        var obs = await context.Observations.FirstAsync();
        targetId = obs.Id;
      }

      // Act: 削除実行
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new OrderRepository(context, config);
        await repo.DeleteObservationAsync(targetId);
      }

      // Assert
      using (var context = CreateInMemoryContext(dbName))
      {
        // 1. 通常のクエリでは取得できないこと（アプリ上は消えている）
        var visibleObs = await context.Observations.FirstOrDefaultAsync(o => o.Id == targetId);
        visibleObs.Should().BeNull();

        // 2. フィルタを無視すれば取得でき、IsDeletedがtrueであること（DBには残っている）
        var deletedObs = await context.Observations
            .IgnoreQueryFilters()
            .Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.Id == targetId);

        deletedObs.Should().NotBeNull();
        deletedObs!.IsDeleted.Should().BeTrue();

        // 3. 子データ(Details)も論理削除されていること
        deletedObs.Details.Should().NotBeEmpty();
        foreach (var detail in deletedObs.Details)
        {
          detail.IsDeleted.Should().BeTrue();
        }
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

      // Act
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
