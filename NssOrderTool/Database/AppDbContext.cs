using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Entities;
using NssOrderTool.Models.Interfaces;

namespace NssOrderTool.Database
{
  public class AppDbContext : DbContext
  {
    // コンストラクタでオプションを受け取る（DIで設定を注入するため）
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // テーブルとの紐づけ
    public DbSet<PlayerEntity> Players { get; set; }
    public DbSet<SequencePairEntity> SequencePairs { get; set; }
    public DbSet<AliasEntity> Aliases { get; set; }
    public DbSet<ObservationEntity> Observations { get; set; }
    public DbSet<ObservationDetailEntity> ObservationDetails { get; set; }
    public DbSet<ArenaSessionEntity> ArenaSessions { get; set; }
    public DbSet<ArenaRoundEntity> ArenaRounds { get; set; }
    public DbSet<ArenaParticipantEntity> ArenaParticipants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // 論理削除のグローバルクエリフィルタを設定
      modelBuilder.Entity<PlayerEntity>().HasQueryFilter(e => !e.IsDeleted);
      modelBuilder.Entity<ObservationEntity>().HasQueryFilter(e => !e.IsDeleted);
      modelBuilder.Entity<ObservationDetailEntity>().HasQueryFilter(e => !e.IsDeleted);
      modelBuilder.Entity<SequencePairEntity>().HasQueryFilter(e => !e.IsDeleted);
      modelBuilder.Entity<AliasEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    // 非同期保存時のフック
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      UpdateTimestamps();
      return base.SaveChangesAsync(cancellationToken);
    }

    // 同期保存時のフック
    public override int SaveChanges()
    {
      UpdateTimestamps();
      return base.SaveChanges();
    }

    // 共通処理: 更新されたEntityを探して日時をセット
    private void UpdateTimestamps()
    {
      // ITimestampを実装しているEntityのうち、追加・変更があったものを抽出
      var entries = ChangeTracker.Entries<ITimestamp>();

      foreach (var entry in entries)
      {
        if (entry.State == EntityState.Added)
        {
          // 新規作成時は CreatedAt と UpdatedAt 両方を現在時刻に
          entry.Entity.CreatedAt = DateTime.Now;
          entry.Entity.UpdatedAt = DateTime.Now;
        }
        else if (entry.State == EntityState.Modified)
        {
          // 更新時は UpdatedAt のみ更新
          entry.Entity.UpdatedAt = DateTime.Now;
        }
      }
    }
  }
}
