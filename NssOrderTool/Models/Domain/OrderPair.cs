namespace NssOrderTool.Models.Domain
{
  // 順序関係を表すレコード
  public record OrderPair(string Predecessor, string Successor);
}
