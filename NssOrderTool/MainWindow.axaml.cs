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

        // 【修正1】初期化 (new) を必ず最初に行う
        // これを先にやらないと、下の EnsureTablesExist などでエラー(NullReference)になります
        _repository = new RankingRepository();
        _extractor = new RelationshipExtractor();
        _sorter = new OrderSorter();

        // 環境表示の更新
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

        // アプリ起動時にテーブル作成 & データ読み込み
        try
        {
            _repository.EnsureTablesExist();
            LoadRanking();
            LoadAliases();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"❌ 初期化エラー: {ex.Message}";
        }
    }

    // 「データを登録」ボタンが押されたとき
    private void RegisterButton_Click(object? sender, RoutedEventArgs e)
    {
        string rawInput = InputBox.Text ?? "";

        if (string.IsNullOrWhiteSpace(rawInput))
        {
            StatusText.Text = "⚠️ 入力が空です";
            return;
        }

        try
        {
            // 【修正2】エイリアス変換ロジックの追加
            // 入力された文字列を一度分解し、辞書にある名前なら書き換える

            // 1. 辞書の取得
            var aliasDict = _repository.GetAliasDictionary();

            // 2. 分解 (カンマ区切り)
            var rawNames = rawInput.Split(',')
                                   .Select(p => p.Trim())
                                   .Where(p => !string.IsNullOrEmpty(p))
                                   .ToList();

            // 3. 変換 (辞書にあれば置換、なければそのまま)
            var convertedNames = new List<string>();
            foreach (var name in rawNames)
            {
                if (aliasDict.TryGetValue(name, out string? target))
                {
                    convertedNames.Add(target); // エイリアスなら正規名に変換
                }
                else
                {
                    convertedNames.Add(name);   // そのまま
                }
            }

            // 4. 再結合 (DB保存 & 抽出用)
            // 例: "A, Taka, B" -> "A, Takahiro, B"
            string normalizedInput = string.Join(", ", convertedNames);


            // --- ここからは変換後の normalizedInput を使う ---

            // 1. 観測ログを保存 (変換後のデータで保存します)
            _repository.AddObservation(normalizedInput);

            // 2. 入力文字を「ペア」に分解
            var pairs = _extractor.ExtractFromInput(normalizedInput);

            if (pairs.Count == 0)
            {
                StatusText.Text = "⚠️ 有効なペアが見つかりませんでした (2名以上入力してください)";
                return;
            }

            // 3. プレイヤーマスタへの登録
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

            // ソート実行 (同率グループ対応版)
            var sortedLayers = _sorter.Sort(allPairs);

            var displayList = new List<string>();
            int currentRank = 1;

            foreach (var group in sortedLayers)
            {
                if (group.Count == 1)
                {
                    displayList.Add($"{currentRank}位 : {group[0]}");
                }
                else
                {
                    // カンマ区切りで結合
                    string names = string.Join(", ", group);
                    displayList.Add($"{currentRank}位 : {names} (推定同率)");
                }

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

        // 2. ダイアログを表示し、結果を待つ
        var result = await dialog.ShowDialog<bool>(this);

        // 3. 結果が true (削除する) の場合のみ実行
        if (result)
        {
            try
            {
                _repository.ClearAllData();
                StatusText.Text = "🗑️ データを全削除しました";
                LoadRanking(); // 空になったランキングを表示
                LoadAliases(); // エイリアス表示もクリア
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

    // --- エイリアス関連 ---

    // エイリアス一覧を読み込んで表示
    private void LoadAliases()
    {
        try
        {
            var dict = _repository.GetAliasDictionary();
            var list = dict.Select(kv => new AliasItem
            {
                AliasName = kv.Key,
                TargetName = kv.Value
            })
            .OrderBy(x => x.AliasName)
            .ToList();

            AliasList.ItemsSource = list;
        }
        catch (Exception ex)
        {
            AliasStatusText.Text = $"❌ 読み込みエラー: {ex.Message}";
        }
    }

    // 「追加する」ボタン
    private void AddAliasButton_Click(object? sender, RoutedEventArgs e)
    {
        string alias = AliasInput.Text?.Trim() ?? "";
        string target = TargetInput.Text?.Trim() ?? "";

        if (string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(target))
        {
            AliasStatusText.Text = "⚠️ 両方の名前を入力してください";
            return;
        }

        try
        {
            _repository.AddAlias(alias, target);

            AliasStatusText.Text = $"✅ 追加しました: {alias} → {target}";
            AliasInput.Text = "";
            TargetInput.Text = "";

            LoadAliases(); // リスト更新
        }
        catch (Exception ex)
        {
            AliasStatusText.Text = $"❌ エラー: {ex.Message}";
        }
    }

    // リスト内の「削除」ボタン
    private void DeleteAliasButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string aliasName)
        {
            try
            {
                _repository.DeleteAlias(aliasName);
                LoadAliases(); // リスト更新
            }
            catch (Exception ex)
            {
                AliasStatusText.Text = $"❌ 削除エラー: {ex.Message}";
            }
        }
    }

    // 「更新」ボタン (エイリアスタブ)
    private void ReloadAliases_Click(object? sender, RoutedEventArgs e)
    {
        LoadAliases();
    }
}

// リスト表示用のデータクラス
public class AliasItem
{
    public string AliasName { get; set; } = "";
    public string TargetName { get; set; } = "";
    public string DisplayText => $"{AliasName} → {TargetName}";
}