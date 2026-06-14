namespace NssOrderTool.Models.Configuration
{
  // JSON設定ファイル用のクラス
  public class AppConfig
  {
    public AppSettings? AppSettings { get; set; }
  }

  public class AppSettings
  {
    public string Environment { get; set; } = "TEST";
    public string DefaultPlayerId { get; set; } = "";
  }
}
