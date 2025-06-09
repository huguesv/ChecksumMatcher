// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public sealed partial class MainViewModel : ObservableRecipient
{
    private readonly IWebLauncherService webLauncherService;

    public MainViewModel(IWebLauncherService webLauncherService)
    {
        ArgumentNullException.ThrowIfNull(webLauncherService);

        this.webLauncherService = webLauncherService;

        this.WebSites =
        [
            new(this.webLauncherService, Localized.HomePageDownloadRedumpTitle, Localized.HomePageDownloadRedumpSubtitle, Localized.HomePageDownloadRedumpDescription, Localized.HomePageDownloadRedumpUrl),
            new(this.webLauncherService, Localized.HomePageDownloadNoIntroTitle, Localized.HomePageDownloadNoIntroSubtitle, Localized.HomePageDownloadNoIntroDescription, Localized.HomePageDownloadNoIntroUrl),
            new(this.webLauncherService, Localized.HomePageDownloadTosecTitle, Localized.HomePageDownloadTosecSubtitle, Localized.HomePageDownloadTosecDescription, Localized.HomePageDownloadTosecUrl),
        ];
    }

    public ObservableCollection<WebSiteViewModel> WebSites { get; }

    public WebSiteViewModel RedumpWebSite => this.WebSites[0];

    public WebSiteViewModel NoIntroWebSite => this.WebSites[1];

    public WebSiteViewModel TosecWebSite => this.WebSites[2];
}
