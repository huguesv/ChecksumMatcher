// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

internal class RestartService : IRestartService
{
    private readonly IHistoryService historyService;

    public RestartService(IHistoryService historyService)
    {
        ArgumentNullException.ThrowIfNull(historyService);

        this.historyService = historyService;
    }

    public async Task RestartAsync()
    {
        if (this.historyService.GetHistoryItems().Any(item => item.PreventQuit && (item.Status == HistoryItemStatus.Pending || item.Status == HistoryItemStatus.InProgress)))
        {
            await ShowOperationInProgressMessageAsync();
        }
        else
        {
            _ = AppInstance.Restart(string.Empty);
        }
    }

    private static async Task ShowOperationInProgressMessageAsync()
    {
        XamlRoot? xamlRoot = (App.MainWindow.Content as FrameworkElement)?.XamlRoot
            ?? throw new InvalidOperationException("Could not find xaml root.");

        ContentDialog errorDialog = new ContentDialog
        {
            Title = Localized.MainWindowCaption,
            Content = Localized.QuitOperationInProgress,
            CloseButtonText = Localized.MessageBoxOK,
            XamlRoot = xamlRoot,
        };

        await errorDialog.ShowAsync();
    }
}
