using System;
using System.Collections.ObjectModel;
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
            LoadAliases();
        }

        // 引数なしコンストラクタ（デザイナー用）
        public AliasEditViewModel()
        {
            _aliasRepo = new AliasRepository();
        }

        [RelayCommand]
        public void LoadAliases()
        {
            try
            {
                Aliases.Clear();
                var list = _aliasRepo.GetAliasesByTarget(TargetName);
                foreach (var alias in list)
                {
                    Aliases.Add(alias);
                }
            }
            catch (Exception)
            {
                // エラーハンドリング（必要ならStatusTextなどを追加）
            }
        }

        [RelayCommand]
        public void DeleteAlias(string alias)
        {
            try
            {
                _aliasRepo.DeleteAlias(alias);
                LoadAliases(); // リスト更新
            }
            catch (Exception)
            {
                // エラーハンドリング
            }
        }
    }
}