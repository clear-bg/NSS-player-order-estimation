using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using NssOrderTool.Database;
using NssOrderTool.Messages;
using NssOrderTool.Models;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NssOrderTool.ViewModels
{
  public partial class OrderEstimationViewModel : ViewModelBase
  {
    // リポジトリ・サービス
    private readonly OrderRepository _orderRepo;
    private readonly PlayerRepository _playerRepo;
    private readonly AliasRepository _aliasRepo;
    private readonly RelationshipExtractor _extractor;
    private readonly OrderSorter _sorter;
    private readonly DbSchemaService _schemaService;
    private readonly GraphVizService _graphViz;

    // --- Bindings (画面と同期するプロパティ) ---

    [ObservableProperty]
    private string _inputText = "";

    [ObservableProperty]
    private string _statusText = "準備完了";

    [ObservableProperty]
    private string _envText = "";

    [ObservableProperty]
    private IBrush _envBadgeColor = Brushes.Gray;

    [ObservableProperty]
    private string _statsText = "";

    public ObservableCollection<string> EstimatedSequence { get; } = new();
    public Func<string, List<string>, Task<bool>>? ConfirmCycleCallback { get; set; }
    public ObservableCollection<HistoryItem> HistoryList { get; } = new();

    private readonly ILogger<OrderEstimationViewModel> _logger;

    public OrderEstimationViewModel(
        OrderRepository orderRepo,
        PlayerRepository playerRepo,
        AliasRepository aliasRepo,
        RelationshipExtractor extractor,
        OrderSorter sorter,
        DbSchemaService schemaService,
        GraphVizService graphViz,
        ILogger<OrderEstimationViewModel> logger)
    {
      _orderRepo = orderRepo;
      _playerRepo = playerRepo;
      _aliasRepo = aliasRepo;
      _extractor = extractor;
      _sorter = sorter;
      _schemaService = schemaService;
      _graphViz = graphViz;
      _logger = logger;

      _ = InitializeAsync();
    }

    // デザイナー用の空コンストラクタ（あるとVSのプレビューが動く）
    public OrderEstimationViewModel()
    {
      /* デザイン時はnullのままで落ちるかもしれないが、一旦許容 */
      _orderRepo = null!;
      _playerRepo = null!;
      _aliasRepo = null!;
      _extractor = null!;
      _sorter = null!;
      _schemaService = null!;
      _graphViz = null!;
      _logger = null!;
    }

    private async Task InitializeAsync()
    {
      try
      {
        // テーブル作成 (ここは同期メソッドのままだが、高速なので許容)
        await _schemaService.EnsureTablesExistAsync(); // 👈 await を追加

        UpdateEnvironmentDisplay();

        // 初回読み込み (非同期)
        await LoadOrderAsync();
        await LoadHistoryAsync();
      }
      catch (Exception ex)
      {
        StatusText = $"❌ 初期化エラー: {ex.Message}";
        _logger.LogError(ex, "初期化処理中にエラーが発生しました。");
      }
    }

    private void UpdateEnvironmentDisplay()
    {
      // 環境名はメモリ上の設定を読むだけなので同期でOK
      string envName = _orderRepo.GetEnvironmentName();
      EnvText = envName;
      EnvBadgeColor = (envName == "PROD") ? Brushes.DarkRed : Brushes.Green;
    }

    // --- Commands (ボタン処理) ---

    [RelayCommand]
    private async Task Register()
    {
      if (string.IsNullOrWhiteSpace(InputText)) return;
      if (IsBusy) return;

      IsBusy = true;
      try
      {
        // 1. エイリアス辞書を非同期で取得して正規化
        var aliasDict = await _aliasRepo.GetAliasDictionaryAsync();
        string normalizedInput = _extractor.NormalizeInput(InputText, aliasDict);

        // 2. ペア分解
        var newPairs = _extractor.ExtractFromInput(normalizedInput);
        if (newPairs.Count == 0)
        {
          StatusText = "⚠️ 有効なペアが見つかりませんでした";
          return;
        }


        // 3. OrderSorterに追加したメソッドで閉路を探す
        var existingPairs = await _orderRepo.GetAllPairsAsync();

        foreach (var pair in newPairs)
        {
          // 「A -> B」を追加しようとしているとき、既に「B -> ... -> A」という道があるか？
          // あるなら、今回の追加によって閉路が完成してしまうことになる。
          var reversePath = _sorter.FindPath(existingPairs, pair.Successor, pair.Predecessor);

          if (reversePath != null)
          {
            // 閉路完成！ (例: B -> C -> A) に、今回の A (始点) を足して B -> C -> A -> B と表示する
            reversePath.Add(pair.Successor);

            if (ConfirmCycleCallback != null)
            {
              bool proceed = await ConfirmCycleCallback(normalizedInput, reversePath);
              if (!proceed)
              {
                StatusText = "🚫 登録をキャンセルしました";
                return;
              }
            }
            // 1つでも矛盾が見つかってユーザーが許可したら、他のペアのチェックは省略して進む（または全件チェックも可）
            break;
          }
        }

        // 4. プレイヤー登録 (先に実行！)
        // newPairs からプレイヤー名を抽出
        var playerNames = newPairs.Select(p => p.Predecessor)
                               .Concat(newPairs.Select(p => p.Successor))
                               .Distinct();

        // まずプレイヤーマスタに存在しない名前を登録する
        await _playerRepo.RegisterPlayersAsync(playerNames);

        // 5. 観測データ保存 (プレイヤーが存在する状態なので成功する)
        await _orderRepo.AddObservationAsync(normalizedInput);

        // 6. ペア関係更新
        await _orderRepo.UpdatePairsAsync(newPairs);

        WeakReferenceMessenger.Default.Send(new DatabaseUpdatedMessage());

        // メッセージ更新
        if (InputText != normalizedInput)
          StatusText = $"✅ 登録完了 (変換あり): \n'{InputText}' \n→ '{normalizedInput}'";
        else
          StatusText = $"✅ 登録完了: {newPairs.Count} 件の関係を更新しました";

        InputText = ""; // 入力欄クリア

        // リスト再読み込み
        await LoadOrderAsync();
        await LoadHistoryAsync();

        // 完了ログ
        _logger.LogInformation("登録処理が完了しました。");
      }
      catch (Exception ex)
      {
        StatusText = $"❌ エラー: {ex.Message}";
        _logger.LogError(ex, "登録処理中にエラーが発生しました。入力値: {InputText}", InputText);
      }
      finally
      {
        IsBusy = false;
      }
    }

    private async Task LoadHistoryAsync()
    {
      try
      {
        var entities = await _orderRepo.GetRecentObservationsAsync();
        HistoryList.Clear();
        foreach (var e in entities)
        {
          var reconstructedList = string.Join(", ", e.Details
              .OrderBy(d => d.OrderIndex)
              .Select(d => d.PlayerId));

          HistoryList.Add(new HistoryItem
          {
            Id = e.Id,
            Timestamp = e.ObservationTime,
            Content = reconstructedList
          });
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "履歴読み込みに失敗しました");
      }
    }

    [RelayCommand]
    private async Task DeleteHistory(HistoryItem item)
    {
      if (item == null || IsBusy) return;

      IsBusy = true;
      try
      {
        var pairsToDecrement = _extractor.ExtractFromInput(item.Content);
        await _orderRepo.UndoObservationAsync(item.Id, pairsToDecrement);

        StatusText = $"✅ 履歴を取り消しました: {item.Content}";

        await LoadOrderAsync();
        await LoadHistoryAsync();
      }
      catch (Exception ex)
      {
        StatusText = $"❌ 取り消しエラー: {ex.Message}";
        _logger.LogError(ex, "履歴削除に失敗しました");
      }
      finally
      {
        IsBusy = false;
      }
    }

    [RelayCommand]
    private async Task Reload()
    {
      if (IsBusy) return;
      IsBusy = true;
      try
      {
        await LoadOrderAsync();
        await LoadHistoryAsync();
      }
      finally
      {
        IsBusy = false;
      }
    }

    // Viewのコードビハインドから呼ばれるメソッド (全削除)
    public async Task PerformClearAsync()
    {
      if (IsBusy) return;
      IsBusy = true;
      try
      {
        await _orderRepo.ClearAllDataAsync();
        await _playerRepo.ClearAllAsync();
        await _aliasRepo.ClearAllAsync();

        _orderRepo.ResetTracking();
        _playerRepo.ResetTracking();
        _aliasRepo.ResetTracking();

        StatusText = "🗑️ データを全削除しました";
        await LoadOrderAsync();
        await LoadHistoryAsync();
      }
      catch (Exception ex)
      {
        StatusText = $"❌ 削除エラー: {ex.Message}";
      }
      finally
      {
        IsBusy = false;
      }
    }

    private async Task LoadOrderAsync()
    {
      try
      {
        // DBから全ペアを非同期取得
        var allPairs = await _orderRepo.GetAllPairsAsync();

        // ソート計算 (オンメモリ処理)
        var sortedLayers = _sorter.Sort(allPairs);

        // 統計情報更新
        int totalPlayers = sortedLayers.Sum(layer => layer.Count);
        int totalPairs = allPairs.Count;
        StatsText = $"({totalPlayers} players, {totalPairs} pairs)";

        EstimatedSequence.Clear();
        int currentRank = 1;

        foreach (var group in sortedLayers)
        {
          string line = (group.Count == 1)
              ? $"{currentRank} : {group[0]}"
              : $"{currentRank} : {string.Join(", ", group)} (推定同列)";

          EstimatedSequence.Add(line);
          currentRank++;
        }
        StatusText = "";
      }
      catch (Exception ex)
      {
        StatusText = $"❌ 読み込みエラー: {ex.Message}";
      }
    }

    public async Task<string> GenerateGraphTextAsync()
    {
      if (IsBusy) return "";
      IsBusy = true;
      try
      {
        // 最新データを取得して計算
        var allPairs = await _orderRepo.GetAllPairsAsync();
        var sortedLayers = _sorter.Sort(allPairs);

        // テキスト生成
        var text = _graphViz.GenerateMermaid(allPairs, sortedLayers);

        StatusText = "📋 グラフ定義をクリップボードにコピーしました (Notion等に貼り付け可能)";
        return text;
      }
      catch (Exception ex)
      {
        StatusText = $"❌ グラフ生成エラー: {ex.Message}";
        return "";
      }
      finally
      {
        IsBusy = false;
      }
    }
  }
}
