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
    private AppDbContext CreateInMemoryContext(string dbName)
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(databaseName: dbName)
          .Options;
      return new AppDbContext(options);
    }

    [Fact]
    public async Task AddSession_ShouldSaveSession_Rounds_AndParticipants()
    {
      // Arrange
      var dbName = "Test_AddSession";
      var session = new ArenaSessionEntity
      {
        // 参加者リストを作成
        Participants = new List<ArenaParticipantEntity>
        {
            new ArenaParticipantEntity { PlayerId = "A", SlotIndex = 0 },
            new ArenaParticipantEntity { PlayerId = "B", SlotIndex = 1 }
        },
        Rounds = new List<ArenaRoundEntity>
        {
            new ArenaRoundEntity { RoundNumber = 1, WinningTeam = 1 },
            new ArenaRoundEntity { RoundNumber = 2, WinningTeam = 2 }
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
            .Include(s => s.Participants) // 参加者も含めて取得
            .FirstOrDefaultAsync();

        saved.Should().NotBeNull();

        // 参加者の検証
        saved!.Participants.Should().HaveCount(2);
        saved.Participants.Should().Contain(p => p.PlayerId == "A");

        // ラウンドの検証
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
        await repo.AddSessionAsync(new ArenaSessionEntity { CreatedAt = System.DateTime.Now.AddDays(-1) });
        await repo.AddSessionAsync(new ArenaSessionEntity { CreatedAt = System.DateTime.Now });
      }

      // Act
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new ArenaRepository(context);
        var sessions = await repo.GetAllSessionsAsync();

        // Assert
        sessions.Should().HaveCount(2);
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
        var session = new ArenaSessionEntity();
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
