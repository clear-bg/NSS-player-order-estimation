using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NssOrderTool.Database
{
  public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
  {
    public AppDbContext CreateDbContext(string[] args)
    {
      // SQLite用のOptionsBuilder設定のみに簡素化
      var builder = new DbContextOptionsBuilder<AppDbContext>();
      builder.UseSqlite("Data Source=local_database.db");

      return new AppDbContext(builder.Options);
    }
  }
}
