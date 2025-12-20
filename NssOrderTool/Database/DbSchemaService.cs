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
      // マイグレーション
      await _dbContext.Database.MigrateAsync();
    }
  }
}
