// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        this.ViewModel = App.GetService<MainViewModel>();
        this.InitializeComponent();
    }

    private void OnCardRedump(object sender, RoutedEventArgs e)
    {
        this.OpenWebBrowser("http://www.redump.org");
    }

    private void OnCardNoIntro(object sender, RoutedEventArgs e)
    {
        this.OpenWebBrowser("https://no-intro.org/");
    }

    private void OnCardTosec(object sender, RoutedEventArgs e)
    {
        this.OpenWebBrowser("https://www.tosecdev.org/");
    }

    private void OnCreateDatabaseClicked(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(CreateDatabaseViewModel).FullName!);
    }

    private void OnConfigureClicked(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
    }

    private void OnAuditClicked(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(DatabasesViewModel).FullName!);
    }

    private void OnHashClicked(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(HashViewModel).FullName!);
    }

    private void OnOfflineStorageView(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(OfflineExplorerViewModel).FullName!);
    }

    private void OnOfflineStorageCreate(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(CreateOfflineStorageViewModel).FullName!);
    }

    private async void OpenWebBrowser(string url)
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
    }
}
