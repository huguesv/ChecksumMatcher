// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public sealed partial class HistoryItemViewModel : ObservableObject
{
    private readonly INavigationService navigationService;
    private readonly ILogger logger;

    public HistoryItemViewModel(INavigationService navigationService, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(logger);

        this.navigationService = navigationService;
        this.logger = logger;
    }

    public string Id { get; init; } = string.Empty;

    [ObservableProperty]
    public partial bool IsExpanded { get; set; } = true;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Subtitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Details { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StartTime { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EndTime { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsCompleted { get; set; } = false;

    [ObservableProperty]
    public partial HistoryStatus Status { get; set; } = HistoryStatus.Pending;

    public string? NavigationPage { get; init; }

    public string? NavigationParameter { get; init; }

    [RelayCommand]
    private void OpenDetails(HistoryItemViewModel item)
    {
        try
        {
            if (item?.NavigationPage is null)
            {
                return;
            }

            this.navigationService.NavigateTo(item.NavigationPage, item.NavigationParameter);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }
}
