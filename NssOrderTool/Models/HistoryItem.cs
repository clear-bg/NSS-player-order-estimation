using System;

namespace NssOrderTool.Models
{
  public class HistoryItem
  {
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Content { get; set; } = "";

    // リスト表示用フォーマット (例: [12/13 20:00] A, B, C)
    public string DisplayText => $"[{Timestamp:MM/dd HH:mm}] {Content}";
  }
}