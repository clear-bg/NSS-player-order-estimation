using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
          .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
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

    [Fact]
    public async Task UndoObservationAsync_ShouldDecrementFrequency_AndRemoveZeroFrequencyPairs()
    {
      // Arrange
      var dbName = "TestDb_Undo";
      int obsId;

      // 1. データ準備: 観測データとペア(A->B)を登録
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new OrderRepository(context, new AppConfig());

        // ログ登録
        await repo.AddObservationAsync("A, B");
        obsId = (await context.Observations.FirstAsync()).Id;

        // ペア登録 (Frequency = 1)
        await repo.UpdatePairsAsync(new List<OrderPair> { new OrderPair("A", "B") });
      }

      // Act
      // 2. 取り消し実行
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new OrderRepository(context, new AppConfig());

        // 削除対象のペアリスト (A->B)
        var pairsToUndo = new List<OrderPair> { new OrderPair("A", "B") };

        await repo.UndoObservationAsync(obsId, pairsToUndo);
      }

      // Assert
      // 3. 検証: ログもペアも消えているはず (Freq 1 -> 0 なので削除される)
      using (var context = CreateInMemoryContext(dbName))
      {
        // ログが消えたか
        var obsExists = await context.Observations.AnyAsync(o => o.Id == obsId);
        obsExists.Should().BeFalse("ログが削除されているべき");

        // ペアが消えたか
        var pairExists = await context.SequencePairs.AnyAsync(p => p.PredecessorId == "A" && p.SuccessorId == "B");
        pairExists.Should().BeFalse("Frequencyが0になったペアは削除されるべき");
      }
    }

    [Fact]
    public async Task UndoObservationAsync_ShouldKeepPair_IfFrequencyIsGreaterThanZero()
    {
      // Arrange
      var dbName = "TestDb_Undo_Keep";
      int obsIdTarget;

      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new OrderRepository(context, new AppConfig());

        // 2回登録して Frequency = 2 にする
        await repo.UpdatePairsAsync(new List<OrderPair> { new OrderPair("X", "Y") }); // 1回目
        await repo.UpdatePairsAsync(new List<OrderPair> { new OrderPair("X", "Y") }); // 2回目

        // 削除対象のログを作る
        await repo.AddObservationAsync("X, Y");
        obsIdTarget = (await context.Observations.LastAsync()).Id;
      }

      // Act
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new OrderRepository(context, new AppConfig());
        var pairsToUndo = new List<OrderPair> { new OrderPair("X", "Y") };

        await repo.UndoObservationAsync(obsIdTarget, pairsToUndo);
      }

      // Assert
      using (var context = CreateInMemoryContext(dbName))
      {
        var pair = await context.SequencePairs.FirstAsync(p => p.PredecessorId == "X" && p.SuccessorId == "Y");

        // 2 - 1 = 1 なので、レコードは残るはず
        pair.Frequency.Should().Be(1);
      }
    }

    [Fact]
    public async Task MergePlayerIdsAsync_ShouldMergeFrequencies_WhenTargetExists()
    {
      // Arrange
      var dbName = "TestDb_Merge_Exist";
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new OrderRepository(context, new AppConfig());
        // 1. データ準備
        // 旧: Taka -> Kazu (2回)
        // 新: Takahiro -> Kazu (3回)
        await repo.UpdatePairsAsync(new List<OrderPair> { new OrderPair("Taka", "Kazu"), new OrderPair("Taka", "Kazu") });
        await repo.UpdatePairsAsync(new List<OrderPair> { new OrderPair("Takahiro", "Kazu"), new OrderPair("Takahiro", "Kazu"), new OrderPair("Takahiro", "Kazu") });
      }

      // Act
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new OrderRepository(context, new AppConfig());
        // Taka を Takahiro に統合
        await repo.MergePlayerIdsAsync("Taka", "Takahiro");
      }

      // Assert
      using (var context = CreateInMemoryContext(dbName))
      {
        // 旧レコード(Taka)は消えているべき
        var oldExists = await context.SequencePairs.AnyAsync(p => p.PredecessorId == "Taka");
        oldExists.Should().BeFalse();

        // 新レコード(Takahiro)は Frequency が 2+3=5 になっているべき
        var merged = await context.SequencePairs.FirstAsync(p => p.PredecessorId == "Takahiro" && p.SuccessorId == "Kazu");
        merged.Frequency.Should().Be(5);
      }
    }

    [Fact]
    public async Task MergePlayerIdsAsync_ShouldRename_WhenTargetDoesNotExist()
    {
      // Arrange
      var dbName = "TestDb_Merge_New";
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new OrderRepository(context, new AppConfig());
        // 旧: OldName -> B (1回)
        // 新: NewName -> B (まだ存在しない)
        await repo.UpdatePairsAsync(new List<OrderPair> { new OrderPair("OldName", "B") });
      }

      // Act
      using (var context = CreateInMemoryContext(dbName))
      {
        var repo = new OrderRepository(context, new AppConfig());
        await repo.MergePlayerIdsAsync("OldName", "NewName");
      }

      // Assert
      using (var context = CreateInMemoryContext(dbName))
      {
        // 旧レコード消失
        (await context.SequencePairs.AnyAsync(p => p.PredecessorId == "OldName")).Should().BeFalse();

        // 新レコード作成 (Frequency=1)
        var ren = await context.SequencePairs.FirstAsync(p => p.PredecessorId == "NewName");
        ren.SuccessorId.Should().Be("B");
        ren.Frequency.Should().Be(1);
      }
    }
  }
}
