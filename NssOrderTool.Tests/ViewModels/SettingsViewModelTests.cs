using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NssOrderTool.Models;
using NssOrderTool.ViewModels;
using Xunit;

namespace NssOrderTool.Tests.ViewModels
{
  public class SettingsViewModelTests
  {
    [Fact]
    public void ShouldLoadSettings_FromAppConfig_OnInit()
    {
      // Arrange (準備)
      // テスト用の設定データを作成 (SSM設定を削除し、シンプルに)
      var config = new AppConfig
      {
        AppSettings = new AppSettings { Environment = "PROD" }
      };

      // 依存するロガー類のモックを作成
      var mockLogger = new Mock<ILogger<SettingsViewModel>>();
      var mockFactory = new Mock<ILoggerFactory>();

      // Act (実行)
      // ViewModelを初期化（ここでLoadSettingsが走るはず）
      var vm = new SettingsViewModel(config, mockLogger.Object, mockFactory.Object, null!);

      // Assert (検証)
      // configの中身がViewModelのプロパティに反映されているか
      vm.Environment.Should().Be("PROD");
    }
  }
}
