using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NssOrderTool.Models;

namespace NssOrderTool.Services
{
    public class SsmTunnelService : IDisposable
    {
        private readonly SsmSettings _settings;
        private readonly ILogger<SsmTunnelService> _logger;
        private Process? _process;

        public SsmTunnelService(AppConfig config, ILogger<SsmTunnelService> logger)
        {
            _settings = config.SsmSettings ?? new SsmSettings();
            _logger = logger;
        }

        public async Task StartAsync()
        {
            if (!_settings.UseSsm)
            {
                _logger.LogInformation("SSM接続は無効化されています。");
                return;
            }

            _logger.LogInformation("🚀 SSMトンネルを開始しています... (Target: {InstanceId})", _settings.InstanceId);

            var arguments = $"ssm start-session --target {_settings.InstanceId} " +
                            $"--document-name AWS-StartPortForwardingSessionToRemoteHost " +
                            $"--parameters \"{{\\\"host\\\":[\\\"{_settings.RemoteHost}\\\"],\\\"portNumber\\\":[\\\"{_settings.RemotePort}\\\"], \\\"localPortNumber\\\":[\\\"{_settings.LocalPort}\\\"]}}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = "aws", // PATHが通っている前提
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                _process = new Process { StartInfo = startInfo };
                
                // エラー出力監視（デバッグ用）
                _process.ErrorDataReceived += (sender, e) => 
                {
                    if (!string.IsNullOrEmpty(e.Data)) _logger.LogWarning("[AWS CLI Error] {Data}", e.Data);
                };

                _process.Start();
                _process.BeginErrorReadLine();

                // 接続確立を少し待つ (本来は "Waiting for connections" を標準出力で監視するのがベストですが、簡易的に待機)
                await Task.Delay(3000);

                if (_process.HasExited)
                {
                    throw new Exception($"AWS CLIプロセスが即座に終了しました。ExitCode: {_process.ExitCode}");
                }

                _logger.LogInformation("✅ SSMトンネル接続準備完了 (LocalPort: {LocalPort})", _settings.LocalPort);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ SSMトンネルの開始に失敗しました。");
                throw; // 起動できなければアプリを落とすか、エラー処理へ
            }
        }

        public void Dispose()
        {
            if (_process != null && !_process.HasExited)
            {
                _logger.LogInformation("🔌 SSMトンネルを切断しています...");
                try
                {
                    _process.Kill(); // プロセスを強制終了
                    _process.WaitForExit(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "プロセス終了時にエラーが発生しました");
                }
                finally
                {
                    _process.Dispose();
                }
            }
        }
    }
}