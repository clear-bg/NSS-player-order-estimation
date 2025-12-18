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
      // テスト用の設定データを作成
      var config = new AppConfig
      {
        AppSettings = new AppSettings { Environment = "PROD" },
        SsmSettings = new SsmSettings
        {
          UseSsm = true,
          InstanceId = "i-test123",
          LocalPort = 9999,
          RemoteHost = "db.example.com",
          RemotePort = 3306
        }
      };

      // 依存するロガー類のモックを作成
      var mockLogger = new Mock<ILogger<SettingsViewModel>>();
      var mockFactory = new Mock<ILoggerFactory>();

      // Act (実行)
      // ViewModelを初期化（ここでLoadSettingsが走るはず）
      var vm = new SettingsViewModel(config, mockLogger.Object, mockFactory.Object);

      // Assert (検証)
      // configの中身がViewModelのプロパティに反映されているか
      vm.Environment.Should().Be("PROD");
      vm.UseSsm.Should().BeTrue();
      vm.InstanceId.Should().Be("i-test123");
      vm.LocalPort.Should().Be(9999);
      vm.RemoteHost.Should().Be("db.example.com");
      vm.RemotePort.Should().Be(3306);
    }
  }
}