// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Notifications;

using Microsoft.Windows.AppNotifications;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Models;

internal sealed class AppNotificationService : IAppNotificationService
{
    private readonly IFileExplorerService fileExplorerService;

    public AppNotificationService(IFileExplorerService fileExplorerService)
    {
        this.fileExplorerService = fileExplorerService;
    }

    ~AppNotificationService()
    {
        this.Unregister();
    }

    public void Initialize()
    {
        AppNotificationManager.Default.NotificationInvoked += this.OnNotificationInvoked;

        if (RuntimeHelper.IsMSIX)
        {
            AppNotificationManager.Default.Register();
        }
        else
        {
            // Note: Requires calling SetCurrentProcessExplicitAppUserModelID early in the app lifecycle.
            string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Square44x44Logo.scale-200.png");
            AppNotificationManager.Default.Register("Woohoo Checksum Matcher", new Uri(iconPath));
        }
    }

    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        if (args.Arguments.TryGetValue(AppNotificationArgumentNames.Action, out var action) &&
            action.Equals(AppNotificationArgumentActions.OpenExplorer, StringComparison.OrdinalIgnoreCase) &&
            args.Arguments.TryGetValue(AppNotificationArgumentNames.File, out var filePath))
        {
            this.fileExplorerService.OpenInExplorer(filePath);
            return;
        }

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            // App.MainWindow.ShowMessageDialogAsync("TODO: Handle notification invocations when your app is already running.", "Notification Invoked");
            App.MainWindow.BringToFront();
        });
    }

    public bool Show(AppNotification notification)
    {
        AppNotificationManager.Default.Show(notification);

        return notification.Id != 0;
    }

    public void Unregister()
    {
        AppNotificationManager.Default.Unregister();
    }
}
