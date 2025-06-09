// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI;

using Microsoft.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using WinUIEx;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Helpers;

public sealed partial class MainWindow : WindowEx
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private readonly UISettings settings;
    private readonly IHistoryService historyService;

    public MainWindow()
    {
        this.InitializeComponent();

        this.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        this.Content = null;
        this.Title = "AppDisplayName".GetLocalized();

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        this.dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        this.settings = new UISettings();
        this.settings.ColorValuesChanged += this.Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

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

    // this handles updating the caption button colors correctly when indows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        this.dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }
}
