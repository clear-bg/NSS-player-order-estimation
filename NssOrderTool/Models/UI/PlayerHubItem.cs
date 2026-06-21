using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NssOrderTool.Models.UI
{
  public partial class PlayerHubItem : ObservableObject
  {
    public string PlayerId { get; set; } = "";

    [ObservableProperty]
    private string _name = "";

    // XAML側で個別のタグとして描画・動的削除できるように ObservableCollection に変更
    public ObservableCollection<string> Aliases { get; } = new();

    public double Rating { get; set; }
    public int TotalMatches { get; set; }
    public string WinRateString { get; set; } = "-";
    public DateTime? LastPlayedAt { get; set; }

    public string Memo { get; set; } = string.Empty;
  }
}
