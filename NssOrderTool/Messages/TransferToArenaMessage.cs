using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace NssOrderTool.Messages
{
  /// <summary>
  /// 推定されたプレイヤー順序をアリーナ集計画面へ転送するためのメッセージ
  /// </summary>
  public class TransferToArenaMessage : ValueChangedMessage<List<string>>
  {
    public TransferToArenaMessage(List<string> playerNames) : base(playerNames)
    {
    }
  }
}
