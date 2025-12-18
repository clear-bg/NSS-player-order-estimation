namespace NssOrderTool.Models
{
  // JSON設定ファイル用のクラス
  public class AppConfig
  {
    public AppSettings? AppSettings { get; set; }
    public SsmSettings? SsmSettings { get; set; }
  }

  public class AppSettings
  {
    public string Environment { get; set; } = "TEST";
  }

  public class SsmSettings
  {
    public bool UseSsm { get; set; } = false;
    public string InstanceId { get; set; } = "";
    public string RemoteHost { get; set; } = "";
    public int RemotePort { get; set; } = 3306;
    public int LocalPort { get; set; } = 3306;
    public string AwsProfile { get; set; } = "default"; // 必要なら
  }
}