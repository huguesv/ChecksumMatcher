// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI;

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;

public sealed partial class MainWindow : WindowEx
{
    private readonly IHistoryService historyService;

    public MainWindow()
    {
        this.InitializeComponent();

        // Set a unique persistence ID for automatic state management
        this.PersistenceId = this.GetType().FullName;

        // Extend the content into the title bar and hide the default title bar
        this.ExtendsContentIntoTitleBar = true;

        this.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;

        // Set the custom title bar
        this.SetTitleBar(this.shellControl.TitleBar);

        this.historyService = App.GetService<IHistoryService>();

        this.AppWindow.Closing += this.AppWindow_Closing;
    }

    private void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (this.historyService.GetHistoryItems().Any(item => item.PreventQuit && (item.Status == HistoryItemStatus.Pending || item.Status == HistoryItemStatus.InProgress)))
        {
            args.Cancel = true;

            this.ShowOperationInProgressMessageAsync().FireAndForget();
        }
    }

    private async Task ShowOperationInProgressMessageAsync()
    {
        ContentDialog errorDialog = new ContentDialog
        {
            Title = this.Title,
            Content = Localized.QuitOperationInProgress,
            CloseButtonText = Localized.MessageBoxOK,
            XamlRoot = this.Content.XamlRoot,
        };

        await errorDialog.ShowAsync();
    }
}
