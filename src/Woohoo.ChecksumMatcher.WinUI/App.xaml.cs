// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using WinUIEx;
using Woohoo.ChecksumMatcher.WinUI.Activation;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Services;
using Woohoo.ChecksumMatcher.WinUI.Models;
using Woohoo.ChecksumMatcher.WinUI.Notifications;
using Woohoo.ChecksumMatcher.WinUI.Services;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;
using Woohoo.ChecksumMatcher.WinUI.Views;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        this.InitializeComponent();

        this.Host = Microsoft.Extensions.Hosting.Host.
            CreateDefaultBuilder().
            UseContentRoot(AppContext.BaseDirectory).
            ConfigureServices((context, services) =>
            {
                // Default Activation Handler
                services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

                // Other Activation Handlers
                services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

                // Services
                services.AddSingleton<IAppNotificationService, AppNotificationService>();
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                services.AddTransient<INavigationViewService, NavigationViewService>();

                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IClipboardService, ClipboardService>();
                services.AddSingleton<IDispatcherQueueService, DispatcherQueueService>();

                // Core Services
                services.AddSingleton<IDatabaseFinderService, DatabaseFinderService>();
                services.AddSingleton<IOfflineDiskFinderService, OfflineDiskFinderService>();
                services.AddSingleton<IFileService, FileService>();
                services.AddSingleton<IFilePickerService, FilePickerService>();

                // Views and ViewModels
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<CreateDatabaseViewModel>();
                services.AddTransient<CreateDatabasePage>();
                services.AddTransient<CreateOfflineStorageViewModel>();
                services.AddTransient<CreateOfflineStoragePage>();
                services.AddTransient<OfflineExplorerPage>();
                services.AddTransient<OfflineExplorerViewModel>();
                services.AddTransient<HashViewModel>();
                services.AddTransient<HashPage>();
                services.AddTransient<DatabasesViewModel>();
                services.AddTransient<DatabasesPage>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainPage>();
                services.AddTransient<ShellPage>();
                services.AddTransient<ShellViewModel>();

                // Configuration
                services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
            }).
            Build();

        App.GetService<IAppNotificationService>().Initialize();

        UnhandledException += this.App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        // App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
