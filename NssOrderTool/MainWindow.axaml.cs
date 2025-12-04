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
            // DBから全ペアを取得
            var allPairs = _repository.GetAllPairs();

            // ソート実行
            var sortedList = _sorter.Sort(allPairs);

            // リストボックスに表示
            // (順位をつけて表示用リストを作成)
            var displayList = new List<string>();
            for (int i = 0; i < sortedList.Count; i++)
            {
                displayList.Add($"{i + 1}位 : {sortedList[i]}");
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
        // 本来はここで「本当に削除しますか？」というダイアログを出すべきですが、
        // まずは機能実装を優先して直接削除します。
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
}