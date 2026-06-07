using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace NssOrderTool.Database
{
  public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
  {
    public AppDbContext CreateDbContext(string[] args)
    {
      // appsettings.json を読み込んで環境を取得
      var config = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: true)
          .Build();

      // AppSettingsセクションからEnvironmentを取得（デフォルトはTEST）
      string envName = config.GetSection("AppSettings")["Environment"]?.ToUpper() ?? "TEST";

      // 環境に応じてファイル名を切り替え
      string dbFileName = (envName == "PROD" || envName == "PRODUCTION")
                          ? "local_db_prod.db"
                          : "local_db_test.db";

      var builder = new DbContextOptionsBuilder<AppDbContext>();
      builder.UseSqlite($"Data Source={dbFileName}");

      return new AppDbContext(builder.Options);
    }
  }
}
