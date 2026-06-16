using CommunityToolkit.Mvvm.Messaging.Messages;

namespace NssOrderTool.Messages
{
  /// <summary>
  /// 特定のプレイヤーを選択した状態で「アリーナデータ」画面へ遷移するためのメッセージ
  /// </summary>
  public class TransferToArenaDataMessage : ValueChangedMessage<string>
  {
    // 送信するのはプレイヤーのID（UUID）です
    public TransferToArenaDataMessage(string playerId) : base(playerId)
    {
    }
  }
}
