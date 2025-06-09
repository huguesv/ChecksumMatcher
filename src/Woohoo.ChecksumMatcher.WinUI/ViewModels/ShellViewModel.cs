// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Navigation;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Views;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private object? selected;

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        this.NavigationService = navigationService;
        this.NavigationService.Navigated += this.OnNavigated;
        this.NavigationViewService = navigationViewService;
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
