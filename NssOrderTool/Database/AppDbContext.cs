using Microsoft.EntityFrameworkCore;
using NssOrderTool.Models.Entities;

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
    public DbSet<ArenaSessionEntity> ArenaSessions { get; set; }
    public DbSet<ArenaRoundEntity> ArenaRounds { get; set; }
    public DbSet<ArenaParticipantEntity> ArenaParticipants { get; set; }
  }
}
