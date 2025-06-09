// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using WinUIEx;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Services;
using Woohoo.ChecksumMatcher.WinUI.Activation;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Models;
using Woohoo.ChecksumMatcher.WinUI.Notifications;
using Woohoo.ChecksumMatcher.WinUI.Services;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;
using Woohoo.ChecksumMatcher.WinUI.Views;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
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
                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IAppNotificationService, AppNotificationService>();
                services.AddSingleton<IClipboardService, ClipboardService>();
                services.AddSingleton<IDispatcherQueueService, DispatcherQueueService>();
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IOperationCompletionService, OperationCompletionService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<ISoundService, SoundService>();
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                services.AddSingleton<IWebLauncherService, WebLauncherService>();

                // Services
                services.AddTransient<INavigationViewService, NavigationViewService>();

                // Core Services
                services.AddSingleton<IDatabaseService, DatabaseService>();
                services.AddSingleton<IDateTimeProviderService, DateTimeProviderService>();
                services.AddSingleton<IFileExplorerService, FileExplorerService>();
                services.AddSingleton<IFilePickerService, FilePickerService>();
                services.AddSingleton<IFileService, FileService>();
                services.AddSingleton<IOfflineExplorerService, OfflineExplorerService>();
                services.AddSingleton<IRedumpWebService, RedumpWebService>();

                // Views and ViewModels
                services.AddSingleton<DatabaseCreatePage>();
                services.AddSingleton<DatabaseCreateViewModel>();
                services.AddSingleton<DatabaseLibraryPage>();
                services.AddSingleton<DatabaseLibraryViewModel>();
                services.AddSingleton<HashCalculatorPage>();
                services.AddSingleton<HashCalculatorViewModel>();
                services.AddSingleton<MainPage>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<OfflineExplorerCreateDiskPage>();
                services.AddSingleton<OfflineExplorerCreateDiskViewModel>();
                services.AddSingleton<OfflineExplorerPage>();
                services.AddSingleton<OfflineExplorerViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<ShellPage>();
                services.AddSingleton<ShellViewModel>();

                // Configuration
                services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
            }).
            Build();

        App.GetService<IAppNotificationService>().Initialize();

        this.UnhandledException += this.App_UnhandledException;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

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

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }
}
