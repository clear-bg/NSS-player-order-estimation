using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NssOrderTool.Database
{
  public class DbSchemaService
  {
    private readonly AppDbContext _dbContext;

    public DbSchemaService(AppDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public virtual async Task EnsureTablesExistAsync()
    {
      await _dbContext.Database.EnsureCreatedAsync();

      var createTableSql = @"
        CREATE TABLE IF NOT EXISTS `ArenaSessions` (
            `session_id` int NOT NULL AUTO_INCREMENT,
            `created_at` datetime(6) NOT NULL,
            `player_ids_csv` longtext CHARACTER SET utf8mb4 NOT NULL,
            PRIMARY KEY (`session_id`)
        );

        CREATE TABLE IF NOT EXISTS `ArenaRounds` (
            `round_id` int NOT NULL AUTO_INCREMENT,
            `session_id` int NOT NULL,
            `round_number` int NOT NULL,
            `winning_team` int NOT NULL,
            PRIMARY KEY (`round_id`),
            CONSTRAINT `FK_ArenaRounds_ArenaSessions_session_id`
            FOREIGN KEY (`session_id`)
            REFERENCES `ArenaSessions` (`session_id`)
            ON DELETE CASCADE
        );
      ";

      await _dbContext.Database.ExecuteSqlRawAsync(createTableSql);
    }
  }
}
