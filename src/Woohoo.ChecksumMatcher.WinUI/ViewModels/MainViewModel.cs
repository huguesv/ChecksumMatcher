// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class MainViewModel : ObservableRecipient
{
    public ObservableCollection<DatabaseWebSiteViewModel> WebSites { get; } =
    [
        new(Localized.HomePageDownloadRedumpTitle, Localized.HomePageDownloadRedumpSubtitle, Localized.HomePageDownloadRedumpDescription, Localized.HomePageDownloadRedumpUrl),
        new(Localized.HomePageDownloadNoIntroTitle, Localized.HomePageDownloadNoIntroSubtitle, Localized.HomePageDownloadNoIntroDescription, Localized.HomePageDownloadNoIntroUrl),
        new(Localized.HomePageDownloadTosecTitle, Localized.HomePageDownloadTosecSubtitle, Localized.HomePageDownloadTosecDescription, Localized.HomePageDownloadTosecUrl),
    ];

    public MainViewModel()
    {
    }
}
