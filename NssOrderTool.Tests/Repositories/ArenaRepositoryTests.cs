using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
          .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // 念のため追加
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
        var repository = new ArenaRepository(context, null!);
        await repository.AddSessionAsync(session);
      }

      // Assert
      using (var context = CreateInMemoryContext(dbName))
      {
        var saved = await context.ArenaSessions
            .Include(s => s.Rounds)
            .Include(s => s.Participants)
            .FirstOrDefaultAsync();

        saved.Should().NotBeNull();
        saved!.Participants.Should().HaveCount(2);
        saved.Rounds.Should().HaveCount(2);

        // 監査カラムの確認
        saved.CreatedAt.Should().BeAfter(DateTime.MinValue);
      }
    }

    [Fact]
    public async Task DeleteSession_LogicalDelete_WorksCorrectly()
    {
      // Arrange
      var dbName = "Test_LogicalDeleteSession";
      int targetId;

      using (var context = CreateInMemoryContext(dbName))
      {
        var repository = new ArenaRepository(context, null!);
        var session = new ArenaSessionEntity();
        await repository.AddSessionAsync(session);
        targetId = session.Id;
      }

      // Act: 削除実行
      using (var context = CreateInMemoryContext(dbName))
      {
        var repository = new ArenaRepository(context, null!);
        await repository.DeleteSessionAsync(targetId);
      }

      // Assert
      using (var context = CreateInMemoryContext(dbName))
      {
        // 1. 通常検索ではヒットしない（アプリ上は消えている）
        var visible = await context.ArenaSessions.FindAsync(targetId);
        visible.Should().BeNull();

        // 2. フィルタ無視ならヒットし、IsDeleted=true（DBには残っている）
        var deleted = await context.ArenaSessions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == targetId);

        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();
      }
    }

    [Fact]
    public async Task DeleteSession_ShouldCascadeLogicalDelete()
    {
      // Arrange
      var dbName = "Test_CascadeDelete";
      int sessionId;

      using (var context = CreateInMemoryContext(dbName))
      {
        var repository = new ArenaRepository(context, null!);
        var session = new ArenaSessionEntity();
        session.Rounds.Add(new ArenaRoundEntity { RoundNumber = 1 });
        session.Participants.Add(new ArenaParticipantEntity { PlayerId = "P1" });

        await repository.AddSessionAsync(session);
        sessionId = session.Id;
      }

      // Act
      using (var context = CreateInMemoryContext(dbName))
      {
        var repository = new ArenaRepository(context, null!);
        await repository.DeleteSessionAsync(sessionId);
      }

      // Assert
      using (var context = CreateInMemoryContext(dbName))
      {
        // 親だけでなく、子データも論理削除されているか確認
        var rounds = await context.ArenaRounds
            .IgnoreQueryFilters()
            .Where(r => r.SessionId == sessionId)
            .ToListAsync();

        var participants = await context.ArenaParticipants
            .IgnoreQueryFilters()
            .Where(p => p.SessionId == sessionId)
            .ToListAsync();

        rounds.Should().NotBeEmpty();
        rounds.All(r => r.IsDeleted).Should().BeTrue("全てのラウンドが論理削除されていること");

        participants.Should().NotBeEmpty();
        participants.All(p => p.IsDeleted).Should().BeTrue("全ての参加者が論理削除されていること");
      }
    }
  }
}
