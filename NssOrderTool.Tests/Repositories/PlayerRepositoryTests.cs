using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Database;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;
using Xunit;

namespace NssOrderTool.Tests.Repositories
{
  public class PlayerRepositoryTests
  {
    private DbContextOptions<AppDbContext> CreateNewContextOptions()
    {
      // テストごとに個別のインメモリDBを作成
      return new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;
    }

    [Fact]
    public async Task GetTopRatedPlayersAsync_ReturnsPlayersSortedByConservativeRating()
    {
      // Arrange
      var options = CreateNewContextOptions();
      using (var context = new AppDbContext(options))
      {
        // テストデータ投入
        // Rating = Mean - 3*Sigma
        context.Players.AddRange(
            new PlayerEntity { Id = "Strong", Name = "Strong", RateMean = 50, RateSigma = 1 }, // 50 - 3 = 47 (1位)
            new PlayerEntity { Id = "Weak", Name = "Weak", RateMean = 10, RateSigma = 1 },     // 10 - 3 = 7  (3位)
            new PlayerEntity { Id = "Average", Name = "Average", RateMean = 25, RateSigma = 1 } // 25 - 3 = 22 (2位)
        );
        await context.SaveChangesAsync();
      }

      // Act
      using (var context = new AppDbContext(options))
      {
        var repo = new PlayerRepository(context);
        var result = await repo.GetTopRatedPlayersAsync(10);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Strong", result[0].Name);  // 47
        Assert.Equal("Average", result[1].Name); // 22
        Assert.Equal("Weak", result[2].Name);    // 7
      }
    }

    [Fact]
    public async Task GetTopRatedPlayersAsync_ExcludesDeletedPlayers()
    {
      // Arrange
      var options = CreateNewContextOptions();
      using (var context = new AppDbContext(options))
      {
        context.Players.AddRange(
            new PlayerEntity { Id = "Active", Name = "Active", RateMean = 50, RateSigma = 1, IsDeleted = false },
            new PlayerEntity { Id = "Deleted", Name = "Deleted", RateMean = 100, RateSigma = 1, IsDeleted = true } // レートは高いが削除済み
        );
        await context.SaveChangesAsync();
      }

      // Act
      using (var context = new AppDbContext(options))
      {
        var repo = new PlayerRepository(context);
        var result = await repo.GetTopRatedPlayersAsync(10);

        // Assert
        Assert.Single(result);
        Assert.Equal("Active", result[0].Name);
      }
    }
  }
}
