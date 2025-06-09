// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public sealed partial class WebSiteViewModel : ObservableObject
{
    private readonly IWebLauncherService webLauncherService;
    private readonly ILogger logger;

    public WebSiteViewModel(
        IWebLauncherService webLauncherService,
        ILogger logger,
        string title,
        string subtitle,
        string description,
        string url)
    {
        ArgumentNullException.ThrowIfNull(webLauncherService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(subtitle);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(url);

        this.webLauncherService = webLauncherService;
        this.logger = logger;
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
    private async Task OpenWebBrowserAsync()
    {
        try
        {
            await this.webLauncherService.LaunchUriAsync(new Uri(this.Url));
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }
}
