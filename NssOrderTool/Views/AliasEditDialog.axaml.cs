using Avalonia.Controls;
using Avalonia.Interactivity;
using NssOrderTool.ViewModels;
using NssOrderTool.Repositories;

namespace NssOrderTool.Views
{
  public partial class AliasEditDialog : Window
  {
    public AliasEditDialog(string targetName, AliasRepository repo)
    {
      InitializeComponent();
      // ViewModel�ɓn��
      DataContext = new AliasEditViewModel(targetName, repo);
    }

    // �f�U�C�i�[�p�Ȃ�
    public AliasEditDialog()
    {
      InitializeComponent();
    }

    // ����{�^��������View�̐Ӗ��Ƃ��Ă����Ɏc��
    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
      Close();
    }
  }
}