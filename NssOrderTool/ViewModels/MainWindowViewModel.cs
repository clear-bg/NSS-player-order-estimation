using CommunityToolkit.Mvvm.ComponentModel;

namespace NssOrderTool.ViewModels
{
  public partial class MainWindowViewModel : ViewModelBase
  {
    [ObservableProperty]
    private string _title = "NSS 順序推定ツール (C#)";

    // 将来的にここに「現在のタブインデックス」や「ステータスバーの文字」など
    // アプリ全体で共有したいデータを持たせることができます。

    public MainWindowViewModel()
    {
      // 初期化処理があればここに記述
    }
  }
}
