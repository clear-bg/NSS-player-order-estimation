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
      await _dbContext.Database.MigrateAsync();
    }

    public async Task GeneratePlantUmlFilesAsync(string outputDirectory)
    {
      if (!Directory.Exists(outputDirectory))
      {
        Directory.CreateDirectory(outputDirectory);
      }

      var designTimeModel = _dbContext.GetService<IDesignTimeModel>().Model;
      var entityTypes = designTimeModel.GetEntityTypes();

      foreach (var entityType in entityTypes)
      {
        var tableName = entityType.GetTableName();
        if (string.IsNullOrEmpty(tableName)) continue;

        var tableComment = EscapeForPlantUml(entityType.GetComment() ?? "");

        var sb = new StringBuilder();
        sb.AppendLine("@startuml");
        sb.AppendLine("!include ../../helper.puml");
        sb.AppendLine();
        sb.AppendLine($"entity table_desc({tableName}, {tableComment}) {{");

        var orderedProperties = entityType.GetProperties()
          .OrderBy(p => p.IsPrimaryKey() ? 0 : p.IsForeignKey() ? 1 : 2)
          .ThenBy(p => p.GetColumnName(), StringComparer.Ordinal);

        foreach (var property in orderedProperties)
        {
          var columnName = property.GetColumnName();
          var columnType = property.GetColumnType() ?? property.ClrType.Name.ToLower();
          var columnComment = EscapeForPlantUml(property.GetComment() ?? "");

          // helper.puml の仕様に合わせて引数を調整
          if (property.IsPrimaryKey())
          {
            // pk は2引数（コメント枠なし）
            sb.AppendLine($"    pk({columnName}, {columnType})");
          }
          else if (property.IsForeignKey())
          {
            // fk は6引数
            sb.AppendLine($"    fk({columnName}, '', '', '', '', {columnType})");
          }
          else
          {
            // col は3引数
            if (string.IsNullOrEmpty(columnComment))
            {
              sb.AppendLine($"    col({columnName}, {columnType})");
            }
            else
            {
              sb.AppendLine($"    col({columnName}, {columnType}, '{columnComment}')");
            }
          }
        }

        sb.AppendLine("}");
        sb.AppendLine("@enduml");

        var filePath = Path.Combine(outputDirectory, $"{tableName}.puml");
        await File.WriteAllTextAsync(filePath, sb.ToString(), new UTF8Encoding(false)); // BOMなしを指定
      }
    }

    // PlantUMLのマクロ引数はシングルクォートで囲むため、コメント内の ' があると
    // 引数の区切りとして誤解釈されてしまう。表示上問題の少い全角クォートに置き換える。
    private static string EscapeForPlantUml(string text)
    {
      return text.Replace("'", "’");
    }
  }
}
