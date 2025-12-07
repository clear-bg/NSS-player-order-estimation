using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NssOrderTool.Repositories;

namespace NssOrderTool.ViewModels
{
    public partial class AliasEditViewModel : ViewModelBase
    {
        private readonly AliasRepository _aliasRepo;

        [ObservableProperty]
        private string _targetName = "";

        // エイリアス一覧（文字列のリスト）
        public ObservableCollection<string> Aliases { get; } = new();

        public AliasEditViewModel(string targetName, AliasRepository aliasRepo)
        {
            _targetName = targetName;
            _aliasRepo = aliasRepo; // 注入されたものを使う

            _ = LoadAliasesAsync();
        }

        // デザイナー用
        public AliasEditViewModel()
        {
            _aliasRepo = null!;
        }

        [RelayCommand]
        public async Task LoadAliasesAsync()
        {
            try
            {
                Aliases.Clear();
                var list = await _aliasRepo.GetAliasesByTargetAsync(TargetName);
                foreach (var alias in list)
                {
                    Aliases.Add(alias);
                }
            }
            catch (Exception)
            {
                // エラー時は何もしないか、必要ならプロパティで通知
            }
        }

        [RelayCommand]
        public async Task DeleteAlias(string alias)
        {
            try
            {
                await _aliasRepo.DeleteAliasAsync(alias);
                await LoadAliasesAsync(); // リスト更新
            }
            catch (Exception)
            {
                // エラーハンドリング
            }
        }
    }
}