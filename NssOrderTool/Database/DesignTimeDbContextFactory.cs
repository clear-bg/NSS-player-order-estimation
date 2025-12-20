using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using DotNetEnv;
using NssOrderTool.Models;

namespace NssOrderTool.Database
{
  public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
  {
    public AppDbContext CreateDbContext(string[] args)
    {
      // 1. 環境変数 (.env) の読み込み
      // コマンド実行ディレクトリからの相対パスで探す
      Env.Load();

      // 2. appsettings.json の読み込み (もし必要なら)
      var config = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: true)
          .Build();

      // 3. 接続文字列の構築
      // Design時は基本的に TEST環境とみなして接続文字列を作る、または .env の値を優先する
      string dbHost = Env.GetString("DB_HOST", "127.0.0.1");
      string dbPort = Env.GetString("DB_PORT", "3306");
      string dbUser = Env.GetString("DB_USER", "root");
      string dbPass = Env.GetString("DB_PASSWORD", "");

      // 環境変数でDB名が指定されていなければ、デフォルトでテスト用DB名を使う
      string dbName = Env.GetString("DB_NAME_TEST", "player_order_data_test");

      var connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};User={dbUser};Password={dbPass};Pooling=True;";

      // 4. OptionsBuilder の設定
      var builder = new DbContextOptionsBuilder<AppDbContext>();
      builder.UseMySql(
          connectionString,
          new MySqlServerVersion(new Version(8, 0, 0)) // 8.0系固定
      );

      return new AppDbContext(builder.Options);
    }
  }
}
