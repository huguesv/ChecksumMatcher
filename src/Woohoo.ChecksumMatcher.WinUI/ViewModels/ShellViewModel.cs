// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Navigation;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Views;

public sealed partial class ShellViewModel : ObservableRecipient, IDisposable
{
    private readonly DisposableBag disposables = DisposableBag.Create<ShellViewModel>();

    public ShellViewModel(
        INavigationService navigationService,
        INavigationViewService navigationViewService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(navigationViewService);

        this.NavigationService = navigationService;
        this.NavigationViewService = navigationViewService;

        this.NavigationService.Navigated += this.OnNavigated;
        this.disposables.Add(() => this.NavigationService.Navigated -= this.OnNavigated);
    }

    [ObservableProperty]
    public partial bool IsBackEnabled { get; set; }

    [ObservableProperty]
    public partial object? Selected { get; set; }

    public INavigationService NavigationService { get; }

    public INavigationViewService NavigationViewService { get; }

    public void Dispose()
    {
        this.disposables.TryDispose();
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        this.IsBackEnabled = this.NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            this.Selected = this.NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = this.NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            this.Selected = selectedItem;
        }
    }
}
