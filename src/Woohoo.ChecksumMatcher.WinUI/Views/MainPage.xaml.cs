// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.ViewModel = App.GetService<MainViewModel>();
        this.InitializeComponent();
    }

    public MainViewModel ViewModel { get; }

    private void OnCreateDatabaseClicked(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(DatabaseCreateViewModel).FullName!);
    }

    private void OnConfigureClicked(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
    }

    private void OnAuditClicked(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(DatabaseLibraryViewModel).FullName!);
    }

    private void OnHashClicked(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(HashCalculatorViewModel).FullName!);
    }

    private void OnOfflineStorageView(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(OfflineExplorerViewModel).FullName!);
    }

    private void OnOfflineStorageCreate(object sender, RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(OfflineExplorerCreateDiskViewModel).FullName!);
    }
}
