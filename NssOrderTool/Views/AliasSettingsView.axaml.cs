using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using NssOrderTool.Repositories;
using NssOrderTool.Models;

namespace NssOrderTool.Views
{
    public partial class AliasSettingsView : UserControl
    {
        private readonly AliasRepository _aliasRepo;

        public AliasSettingsView()
        {
            InitializeComponent();
            _aliasRepo = new AliasRepository();
            LoadAliases();
        }

        private void LoadAliases()
        {
            try
            {
                var dict = _aliasRepo.GetAliasDictionary();
                var list = dict.GroupBy(kv => kv.Value)
                               .Select(g => new AliasGroupItem
                               {
                                   TargetName = g.Key,
                                   Aliases = g.Select(kv => kv.Key).OrderBy(a => a).ToList()
                               })
                               .OrderBy(x => x.TargetName)
                               .ToList();

                AliasList.ItemsSource = list;
            }
            catch (Exception ex)
            {
                AliasStatusText.Text = $"❌ 読み込みエラー: {ex.Message}";
            }
        }

        private void AddAliasButton_Click(object? sender, RoutedEventArgs e)
        {
            string rawAliases = AliasInput.Text ?? "";
            string target = TargetInput.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(rawAliases) || string.IsNullOrEmpty(target))
            {
                AliasStatusText.Text = "⚠️ 両方の名前を入力してください";
                return;
            }

            var aliasList = rawAliases.Split(',')
                                      .Select(a => a.Trim())
                                      .Where(a => !string.IsNullOrEmpty(a))
                                      .ToList();

            if (aliasList.Count == 0) return;

            try
            {
                int successCount = 0;
                List<string> errors = new List<string>();

                foreach (var alias in aliasList)
                {
                    if (alias.Equals(target, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add($"{alias} (正規名と同じ)");
                        continue;
                    }

                    try
                    {
                        _aliasRepo.AddAlias(alias, target);
                        successCount++;
                    }
                    catch
                    {
                        errors.Add($"{alias} (重複など)");
                    }
                }

                if (errors.Count == 0)
                {
                    AliasStatusText.Text = $"✅ {successCount} 件追加しました";
                    AliasInput.Text = "";
                }
                else
                {
                    AliasStatusText.Text = $"⚠️ {successCount} 件追加, エラー: {string.Join(", ", errors)}";
                }
                LoadAliases();
            }
            catch (Exception ex)
            {
                AliasStatusText.Text = $"❌ エラー: {ex.Message}";
            }
        }

        private void DeleteAliasButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AliasGroupItem group)
            {
                try
                {
                    foreach (var alias in group.Aliases)
                    {
                        _aliasRepo.DeleteAlias(alias);
                    }
                    AliasStatusText.Text = $"🗑️ '{group.TargetName}' のエイリアスを削除しました";
                    LoadAliases();
                }
                catch (Exception ex)
                {
                    AliasStatusText.Text = $"❌ 削除エラー: {ex.Message}";
                }
            }
        }

        private void ReloadAliases_Click(object? sender, RoutedEventArgs e)
        {
            LoadAliases();
        }

        private async void EditGroupButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AliasGroupItem group)
            {
                var window = TopLevel.GetTopLevel(this) as Window;
                if (window == null) return;

                var dialog = new AliasEditDialog(group.TargetName);
                await dialog.ShowDialog(window);
                LoadAliases();
            }
        }
    }
}