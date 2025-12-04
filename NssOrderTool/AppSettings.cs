namespace NssOrderTool.Services
{
    // JSONの構造に合わせたクラス定義
    public class AppConfig
    {
        public AppSettings? AppSettings { get; set; }
    }

    public class AppSettings
    {
        public string Environment { get; set; } = "TEST"; // デフォルト値
    }
}