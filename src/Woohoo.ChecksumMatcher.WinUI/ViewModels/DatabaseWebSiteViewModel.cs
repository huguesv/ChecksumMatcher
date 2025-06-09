// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.System;

public partial class DatabaseWebSiteViewModel : ObservableObject
{
    public DatabaseWebSiteViewModel(string title, string subtitle, string description, string url)
    {
        this.Title = title;
        this.Subtitle = subtitle;
        this.Description = description;
        this.Url = url;
    }

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    [RelayCommand]
    public async Task OpenWebBrowser()
    {
        await Launcher.LaunchUriAsync(new Uri(this.Url));
    }
}
