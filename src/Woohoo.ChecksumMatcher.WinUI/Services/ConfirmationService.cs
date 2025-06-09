// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

internal sealed class ConfirmationService : IConfirmationService
{
    public async Task<bool> ShowAsync(string title, string message, bool defaultToCancel = false, string? confirmButtonText = null, string? cancelButtonText = null)
    {
        var dlg = new ContentDialog()
        {
            Title = title,
            Content = message,
            PrimaryButtonText = confirmButtonText ?? Localized.ConfirmationDialogOK,
            CloseButtonText = cancelButtonText ?? Localized.ConfirmationDialogCancel,
        };

        XamlRoot? xamlRoot = (App.MainWindow.Content as FrameworkElement)?.XamlRoot
            ?? throw new InvalidOperationException("Could not find xaml root.");

        dlg.DefaultButton = defaultToCancel ? ContentDialogButton.Close : ContentDialogButton.Primary;
        dlg.XamlRoot = xamlRoot;

        var result = await dlg.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}
