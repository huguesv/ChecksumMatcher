// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class OfflineExplorerPage : Page
{
    public OfflineExplorerPage()
    {
        this.ViewModel = App.GetService<OfflineExplorerViewModel>();

        this.InitializeComponent();
    }

    public OfflineExplorerViewModel ViewModel { get; }

    private void TreeViewItem_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (sender is TreeViewItem treeViewItem)
        {
            treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
        }
    }

    private void TreeViewItem_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key != Windows.System.VirtualKey.Enter)
        {
            return;
        }

        if (sender is TreeViewItem treeViewItem)
        {
            treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
        }
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        var folderViewModel = (args.Item as OfflineExplorerBreadcrumbViewModel)?.Item;
        if (folderViewModel is not null)
        {
            this.GotoFolder(folderViewModel, keyboardFocus: false, resetSelectedFile: false);
        }
    }

    private void ListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement element)
        {
            if (element.DataContext is OfflineExplorerFolderViewModel folderViewModel)
            {
                this.GotoFolder(folderViewModel, keyboardFocus: false, resetSelectedFile: true);
            }
        }
    }

    private void FoldersAndFilesListView_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            var selection = this.FoldersAndFilesListView.SelectedItem;
            if (selection is OfflineExplorerFolderViewModel folderViewModel)
            {
                this.GotoFolder(folderViewModel, keyboardFocus: true, resetSelectedFile: true);
            }
        }
        else if (e.Key == Windows.System.VirtualKey.Back)
        {
            var folderViewModel = this.ViewModel.SelectedDisk?.SelectedFolder?.ParentViewModel;
            if (folderViewModel is not null)
            {
                this.GotoFolder(folderViewModel, keyboardFocus: true, resetSelectedFile: true);
            }
        }
    }

    private void FoldersAndFilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 1)
        {
            this.FoldersAndFilesListView.ScrollIntoView(e.AddedItems[0]);
        }
    }

    private void GotoFolder(OfflineExplorerFolderViewModel folderViewModel, bool keyboardFocus, bool resetSelectedFile)
    {
        if (this.ViewModel.SelectedDisk is null)
        {
            return;
        }

        this.ViewModel.SelectedDisk.SelectedFolder = folderViewModel;
        if (this.ViewModel.SelectedDisk.SelectedFolder is null)
        {
            return;
        }

        if (resetSelectedFile || this.ViewModel.SelectedDisk.SelectedFolder.SelectedFile is null)
        {
            this.ViewModel.SelectedDisk.SelectedFolder.SelectedFile = this.ViewModel.SelectedDisk.SelectedFolder.SortedFoldersAndFiles.FirstOrDefault();
        }

        if (this.ViewModel.SelectedDisk.SelectedFolder.SelectedFile is null)
        {
            return;
        }

        this.FoldersAndFilesListView.ScrollIntoView(this.ViewModel.SelectedDisk.SelectedFolder.SelectedFile);

        if (keyboardFocus)
        {
            this.FoldersAndFilesListView.Focus(FocusState.Keyboard);
        }
    }
}
