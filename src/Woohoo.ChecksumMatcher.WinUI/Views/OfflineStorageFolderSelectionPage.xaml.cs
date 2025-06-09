// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class OfflineStorageFolderSelectionPage : Page
{
    public OfflineStorageFolderSelectionPage()
    {
        this.ViewModel = App.GetService<OfflineStorageFolderSelectionViewModel>();

        this.InitializeComponent();
    }

    public OfflineStorageFolderSelectionViewModel ViewModel { get; }
}
