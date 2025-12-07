namespace NssOrderTool.Models
{
    // JSON設定ファイル用のクラス
    public class AppConfig
    {
        public AppSettings? AppSettings { get; set; }
    }

    public class AppSettings
    {
        public string Environment { get; set; } = "TEST";
    }
}