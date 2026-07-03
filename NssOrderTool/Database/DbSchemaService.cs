using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

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

    /// <summary>
    /// EF Coreのモデルから PlantUML (.puml) ファイルを1テーブル1ファイルで出力します。
    /// </summary>
    /// <param name="outputDirectory">出力先のディレクトリパス (例: docs/database/plantuml/src/tables)</param>
    public async Task GeneratePlantUmlFilesAsync(string outputDirectory)
    {
      // 出力先ディレクトリが存在しない場合は作成
      if (!Directory.Exists(outputDirectory))
      {
        Directory.CreateDirectory(outputDirectory);
      }

      // ★修正: 実行時最適化モデルではなく、コメント等が含まれるデザイン時モデルを取得する
      var designTimeModel = _dbContext.GetService<IDesignTimeModel>().Model;
      var entityTypes = designTimeModel.GetEntityTypes();

      foreach (var entityType in entityTypes)
      {
        var tableName = entityType.GetTableName();
        if (string.IsNullOrEmpty(tableName)) continue;

        // クラスに付与した [Comment] の内容を取得
        var tableComment = entityType.GetComment() ?? "";

        var sb = new StringBuilder();
        sb.AppendLine("@startuml");

        // src/tables/ から見て2つ上の階層にある helper.puml を読み込む
        sb.AppendLine("!include ../../helper.puml");
        sb.AppendLine();
        sb.AppendLine($"entity table_desc({tableName}, {tableComment}) {{");

        foreach (var property in entityType.GetProperties())
        {
          var columnName = property.GetColumnName();

          // データベース上の型名（int, varcharなど）を取得。取れなければC#の型名を使用。
          var columnType = property.GetColumnType() ?? property.ClrType.Name.ToLower();

          // プロパティに付与した [Comment] の内容を取得
          var columnComment = property.GetComment() ?? "";

          // PK, FK, 一般カラムを判定してマクロを出力
          if (property.IsPrimaryKey())
          {
            sb.AppendLine($"    pk({columnName}, {columnType}, '{columnComment}')");
          }
          else if (property.IsForeignKey())
          {
            sb.AppendLine($"    fk({columnName}, {columnType}, '{columnComment}')");
          }
          else
          {
            sb.AppendLine($"    col({columnName}, {columnType}, '{columnComment}')");
          }
        }

        sb.AppendLine("}");
        sb.AppendLine("@enduml");

        // ファイル名は "テーブル名.puml"
        var filePath = Path.Combine(outputDirectory, $"{tableName}.puml");
        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
      }
    }
  }
}
