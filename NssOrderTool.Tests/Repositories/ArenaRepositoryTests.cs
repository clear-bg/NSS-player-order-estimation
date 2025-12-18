using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Database;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;
using Xunit;

namespace NssOrderTool.Tests.Repositories
{
  public class ArenaRepositoryTests
  {
    // テストごとにクリーンなDBコンテキストを作成するヘルパー
    private AppDbContext CreateInMemoryContext(string dbName)
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(databaseName: dbName)
          .Options;
      return new AppDbContext(options);
    }

    [Fact]
    public async Task AddSession_ShouldSaveSessionAndRounds()
    {
      // Arrange
      var dbName = "Test_AddSession";
      var session = new ArenaSessionEntity
      {
        PlayerIdsCsv = "A,B,C,D,E,F,G,H",
        Rounds = new List<ArenaRoundEntity>
        {
            new ArenaRoundEntity { RoundNumber = 1, WinningTeam = 1 }, // Blue Win
            new ArenaRoundEntity { RoundNumber = 2, WinningTeam = 2 }  // Orange Win
        }
      };

      // Act
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new ArenaRepository(context);
        await repo.AddSessionAsync(session);
      }

      // Assert
      using (var context = CreateInMemoryContext(dbName))
      {
        var saved = await context.ArenaSessions
            .Include(s => s.Rounds)
            .FirstOrDefaultAsync();

        saved.Should().NotBeNull();
        saved!.PlayerIdsCsv.Should().Be("A,B,C,D,E,F,G,H");
        saved.Rounds.Should().HaveCount(2);
        saved.Rounds.First(r => r.RoundNumber == 1).WinningTeam.Should().Be(1);
      }
    }

    [Fact]
    public async Task GetAllSessions_ShouldReturnDescendingOrder()
    {
      // Arrange
      var dbName = "Test_GetAllSessions";
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new ArenaRepository(context);
        // 古いデータ
        await repo.AddSessionAsync(new ArenaSessionEntity { CreatedAt = System.DateTime.Now.AddDays(-1) });
        // 新しいデータ
        await repo.AddSessionAsync(new ArenaSessionEntity { CreatedAt = System.DateTime.Now });
      }

      // Act
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new ArenaRepository(context);
        var sessions = await repo.GetAllSessionsAsync();

        // Assert
        sessions.Should().HaveCount(2);
        // 新しい順（降順）になっているか
        sessions[0].CreatedAt.Should().BeAfter(sessions[1].CreatedAt);
      }
    }

    [Fact]
    public async Task DeleteSession_ShouldRemoveData()
    {
      // Arrange
      var dbName = "Test_DeleteSession";
      int targetId;

      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new ArenaRepository(context);
        var session = new ArenaSessionEntity { PlayerIdsCsv = "DeleteMe" };
        await repo.AddSessionAsync(session);
        targetId = session.Id;
      }

      // Act
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new ArenaRepository(context);
        await repo.DeleteSessionAsync(targetId);
      }

      // Assert
      using (var context = CreateInMemoryContext(dbName))
      {
        var deleted = await context.ArenaSessions.FindAsync(targetId);
        deleted.Should().BeNull();
      }
    }
  }
}
