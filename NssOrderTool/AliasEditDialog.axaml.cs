using Avalonia.Controls;
using Avalonia.Interactivity;
using NssOrderTool.Repositories;
using System;

namespace NssOrderTool
{
    public partial class AliasEditDialog : Window
    {
        private readonly AliasRepository _repository;
        private readonly string _targetName;

        // コンストラクタで「誰の編集か」を受け取る
        public AliasEditDialog(string targetName)
        {
            InitializeComponent();

            // 本当は依存性注入などが望ましいですが、簡易的にここでnewします
            _repository = new AliasRepository();
            _targetName = targetName;

            TargetNameText.Text = _targetName;
            LoadDetails();
        }

        // プレビュー用コンストラクタ (VSデザイナ用)
        public AliasEditDialog()
        {
            InitializeComponent();
            _repository = new AliasRepository();
            _targetName = "Sample";
        }

        private void LoadDetails()
        {
            try
            {
                // Step 1で作ったメソッドを使用
                var aliases = _repository.GetAliasesByTarget(_targetName);
                DetailList.ItemsSource = aliases;
            }
            catch (Exception ex)
            {
                // エラー時はリストにエラーメッセージを1つ入れるなどの簡易対応
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void DeleteOneButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string aliasToDelete)
            {
                try
                {
                    _repository.DeleteAlias(aliasToDelete);
                    LoadDetails(); // リスト更新
                }
                catch
                {
                    // エラーハンドリング (必要ならMessageBoxなど)
                }
            }
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}