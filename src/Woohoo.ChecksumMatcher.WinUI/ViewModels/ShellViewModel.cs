// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Navigation;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Views;

public partial class ShellViewModel : ObservableRecipient
{
    public ShellViewModel(
        INavigationService navigationService,
        INavigationViewService navigationViewService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(navigationViewService);

        this.NavigationService = navigationService;
        this.NavigationService.Navigated += this.OnNavigated;
        this.NavigationViewService = navigationViewService;
    }

    [ObservableProperty]
    public partial bool IsBackEnabled { get; set; }

    [ObservableProperty]
    public partial object? Selected { get; set; }

    public INavigationService NavigationService { get; }

    public INavigationViewService NavigationViewService { get; }

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
