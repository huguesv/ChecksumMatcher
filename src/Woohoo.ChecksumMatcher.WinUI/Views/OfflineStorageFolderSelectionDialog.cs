// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Woohoo.ChecksumMatcher.WinUI.Models;

internal sealed class OfflineStorageFolderSelectionDialog
{
    public OfflineDiskFolder? SelectedDiskFolder { get; private set; }

    public async Task<bool> ShowAsync(XamlRoot xamlRoot)
    {
        var offlineStorageFolderSelectionPage = App.GetService<OfflineStorageFolderSelectionPage>();
        await offlineStorageFolderSelectionPage.ViewModel.EnsureLoadedAsync(CancellationToken.None);

        var dialog = new ContentDialog
        {
            XamlRoot = xamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = Localized.OfflineStorageFolderSelectionDialogTitle,
            Content = offlineStorageFolderSelectionPage,
            PrimaryButtonText = Localized.OfflineStorageFolderSelectionDialogOK,
            CloseButtonText = Localized.OfflineStorageFolderSelectionDialogCancel,
        };

        var binding = new Binding
        {
            Source = offlineStorageFolderSelectionPage.ViewModel,
            Path = new PropertyPath(nameof(offlineStorageFolderSelectionPage.ViewModel.IsSelectionValid)),
            Mode = BindingMode.OneWay,
        };

        dialog.SetBinding(ContentDialog.IsPrimaryButtonEnabledProperty, binding);

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var diskName = offlineStorageFolderSelectionPage.ViewModel.SelectedDisk?.Disk?.Name;
            var folderPath = offlineStorageFolderSelectionPage.ViewModel.SelectedDisk?.SelectedFolder?.FolderPath;

            if (string.IsNullOrEmpty(diskName) || string.IsNullOrEmpty(folderPath))
            {
                this.SelectedDiskFolder = null;
                return false;
            }
            else
            {
                this.SelectedDiskFolder = new OfflineDiskFolder(diskName, folderPath);
                return true;
            }
        }
        else
        {
            this.SelectedDiskFolder = null;
        }

        return false;
    }
}
