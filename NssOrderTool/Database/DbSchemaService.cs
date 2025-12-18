using System.Threading.Tasks;

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
      // シンプルな形に戻す
      await _dbContext.Database.EnsureCreatedAsync();
    }
  }
}
