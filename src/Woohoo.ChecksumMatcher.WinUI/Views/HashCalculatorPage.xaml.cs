// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class HashCalculatorPage : Page
{
    public HashCalculatorPage()
    {
        this.ViewModel = App.GetService<HashCalculatorViewModel>();
        this.InitializeComponent();
    }

    public HashCalculatorViewModel ViewModel { get; }

    private void ScrollViewer_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
    {
        this.HandleDrop(e).FireAndForget();
    }

    private async Task HandleDrop(Microsoft.UI.Xaml.DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count > 0 && items[0] is StorageFile file && !string.IsNullOrEmpty(file.Path))
            {
                await this.ViewModel.ProcessFileAsync(file.Path);
                e.Handled = true;
            }
        }
    }

    private void ScrollViewer_DragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
    {
        if (!this.ViewModel.IsCalculating && e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Link;
        }
        else
        {
            e.AcceptedOperation = DataPackageOperation.None;
        }
    }
}
