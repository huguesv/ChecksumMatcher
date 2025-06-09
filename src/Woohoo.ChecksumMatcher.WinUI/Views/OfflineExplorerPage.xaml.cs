// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Woohoo.ChecksumMatcher.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OfflineExplorerPage : Page
    {
        public OfflineExplorerPage()
        {
            this.ViewModel = App.GetService<OfflineExplorerViewModel>();

            this.InitializeComponent();
        }

        public OfflineExplorerViewModel ViewModel
        {
            get;
        }

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
            this.ViewModel.SelectedFolder = (args.Item as OfflineExplorerBreadcrumbViewModel)?.Item;
            this.ViewModel.SelectedFile = this.ViewModel.SelectedFolder?.SortedFoldersAndFiles.FirstOrDefault();
        }

        private void ListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element)
            {
                if (element.DataContext is OfflineExplorerFolderViewModel folderViewModel)
                {
                    this.ViewModel.SelectedFolder = folderViewModel;
                    this.ViewModel.SelectedFile = this.ViewModel.SelectedFolder?.SortedFoldersAndFiles.FirstOrDefault();

                    if (this.ViewModel.SelectedFile is not null)
                    {
                        this.FoldersAndFilesListView.ScrollIntoView(this.ViewModel.SelectedFile);
                    }

                    this.FoldersAndFilesListView.Focus(FocusState.Keyboard);
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
                    this.ViewModel.SelectedFolder = folderViewModel;
                    this.ViewModel.SelectedFile = this.ViewModel.SelectedFolder?.SortedFoldersAndFiles.FirstOrDefault();

                    if (this.ViewModel.SelectedFile is not null)
                    {
                        this.FoldersAndFilesListView.ScrollIntoView(this.ViewModel.SelectedFile);
                    }

                    this.FoldersAndFilesListView.Focus(FocusState.Keyboard);
                }
            }
            else if (e.Key == Windows.System.VirtualKey.Back)
            {
                if (this.ViewModel.SelectedFolder?.ParentViewModel is not null)
                {
                    this.ViewModel.SelectedFolder = this.ViewModel.SelectedFolder.ParentViewModel;
                    this.ViewModel.SelectedFile = this.ViewModel.SelectedFolder?.SortedFoldersAndFiles.FirstOrDefault();

                    if (this.ViewModel.SelectedFile is not null)
                    {
                        this.FoldersAndFilesListView.Focus(FocusState.Keyboard);
                    }

                    this.FoldersAndFilesListView.ScrollIntoView(this.ViewModel.SelectedFile);
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
    }
}
