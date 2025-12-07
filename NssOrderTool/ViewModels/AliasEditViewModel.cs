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

        public AliasEditViewModel(string targetName)
        {
            _targetName = targetName;
            _aliasRepo = new AliasRepository();

            // 非同期読み込み開始
            _ = LoadAliasesAsync();
        }

        // 引数なしコンストラクタ（デザイナー用）
        public AliasEditViewModel()
        {
            _aliasRepo = new AliasRepository();
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