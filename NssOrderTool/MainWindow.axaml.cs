using Avalonia.Controls;
using Avalonia.Interactivity;
using NssOrderTool.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NssOrderTool;

public partial class MainWindow : Window
{
    private readonly RankingRepository _repository;
    private readonly RelationshipExtractor _extractor;
    private readonly OrderSorter _sorter;

    public MainWindow()
    {
        InitializeComponent();

        // サービス (ロジック) の初期化
        _repository = new RankingRepository();
        _extractor = new RelationshipExtractor();
        _sorter = new OrderSorter();

        string envName = _repository.GetEnvironmentName();
        EnvText.Text = envName;

        if (envName == "PROD")
        {
            EnvBadge.Background = Avalonia.Media.Brushes.DarkRed; // 本番は赤
            this.Title += " (本番環境)";
        }
        else
        {
            EnvBadge.Background = Avalonia.Media.Brushes.Green;   // テストは緑
            this.Title += " (テスト環境)";
        }

        // アプリ起動時にテーブル作成 & ランキング読み込み
        try
        {
            _repository.EnsureTablesExist();
            LoadRanking();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"❌ 初期化エラー: {ex.Message}";
        }
    }

    // 「データを登録」ボタンが押されたとき
    private void RegisterButton_Click(object? sender, RoutedEventArgs e)
    {
        string input = InputBox.Text ?? "";

        if (string.IsNullOrWhiteSpace(input))
        {
            StatusText.Text = "⚠️ 入力が空です";
            return;
        }

        try
        {
            // 1. 観測ログを保存
            _repository.AddObservation(input);

            // 2. 入力文字を「ペア」に分解
            var pairs = _extractor.ExtractFromInput(input);

            if (pairs.Count == 0)
            {
                StatusText.Text = "⚠️ 有効なペアが見つかりませんでした (2名以上入力してください)";
                return;
            }

            // 3. プレイヤーマスタへの登録
            // (ペアに含まれる名前をリストアップして登録)
            var playerNames = pairs.Select(p => p.Predecessor)
                             .Concat(pairs.Select(p => p.Successor))
                             .Distinct();
            _repository.RegisterPlayers(playerNames);

            // 4. DBの関係性を更新
            _repository.UpdatePairs(pairs);

            // 5. 完了メッセージ & ランキング再表示
            StatusText.Text = $"✅ 登録完了: {pairs.Count} 件の関係を更新しました";
            InputBox.Text = ""; // 入力欄をクリア
            LoadRanking();      // ランキング更新
        }
        catch (Exception ex)
        {
            StatusText.Text = $"❌ エラー: {ex.Message}";
        }
    }

    // 「更新」ボタンが押されたとき
    private void ReloadButton_Click(object? sender, RoutedEventArgs e)
    {
        LoadRanking();
    }

    // DBからデータを読み込んでランキングを表示するメソッド
    private void LoadRanking()
    {
        try
        {
            var allPairs = _repository.GetAllPairs();

            // 修正: 戻り値が List<List<string>> になりました
            var sortedLayers = _sorter.Sort(allPairs);

            var displayList = new List<string>();
            int currentRank = 1;

            foreach (var group in sortedLayers)
            {
                // グループ内の人数が1人か複数かで表示を変える
                if (group.Count == 1)
                {
                    displayList.Add($"{currentRank}位 : {group[0]}");
                }
                else
                {
                    // カンマ区切りで結合 (例: "B, D")
                    string names = string.Join(", ", group);
                    displayList.Add($"{currentRank}位 : {names} (推定同率)");
                }

                // 次の順位へ（同率がいても次は+1ランクとするか、人数分飛ばすかは仕様次第ですが、一旦+1で）
                currentRank++;
            }

            RankingList.ItemsSource = displayList;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"❌ 読み込みエラー: {ex.Message}";
        }
    }

    // 「初期化」ボタンが押されたとき
    private async void ClearButton_Click(object? sender, RoutedEventArgs e)
    {
        // 1. 確認ダイアログを作成
        var dialog = new ConfirmationDialog();

        // 2. ダイアログを表示し、結果を待つ (ShowDialog)
        // MainWindow (this) の上に表示する
        var result = await dialog.ShowDialog<bool>(this);

        // 3. 結果が true (削除する) の場合のみ実行
        if (result)
        {
            try
            {
                _repository.ClearAllData();
                StatusText.Text = "🗑️ データを全削除しました";
                LoadRanking(); // 空になったランキングを表示
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ 削除エラー: {ex.Message}";
            }
        }
        else
        {
            StatusText.Text = "キャンセルしました";
        }
    }
}