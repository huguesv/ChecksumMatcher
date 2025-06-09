// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.ViewModels;
using Woohoo.ChecksumMatcher.WinUI.Helpers;

public sealed partial class HistoryViewModel : ObservableRecipient, INavigationAware, IDisposable
{
    private readonly DisposableBag disposables = DisposableBag.Create<HistoryViewModel>();

    private readonly IHistoryService historyService;
    private readonly IDispatcherQueueService dispatcherQueueService;
    private readonly INavigationService navigationService;
    private readonly ILogger logger;
    private readonly IDispatcherQueue dispatcherQueue;

    public HistoryViewModel(
        IHistoryService historyService,
        IDispatcherQueueService dispatcherQueueService,
        INavigationService navigationService,
        ILogger<HistoryViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(historyService);
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(logger);

        this.historyService = historyService;
        this.dispatcherQueueService = dispatcherQueueService;
        this.navigationService = navigationService;
        this.logger = logger;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();

        this.historyService.ItemsReset += this.HistoryService_ItemsReset;
        this.historyService.ItemAdded += this.HistoryService_ItemAdded;
        this.historyService.ItemUpdated += this.HistoryService_ItemUpdated;

        this.disposables.Add(() =>
        {
            this.historyService.ItemsReset -= this.HistoryService_ItemsReset;
            this.historyService.ItemAdded -= this.HistoryService_ItemAdded;
            this.historyService.ItemUpdated -= this.HistoryService_ItemUpdated;
        });

        this.PopulateHistory();
    }

    public ObservableCollection<HistoryItemViewModel> Items { get; set; } = [];

    public void Dispose()
    {
        this.disposables.TryDispose();
    }

    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }

    private HistoryItemViewModel GetHistoryItemViewModel(HistoryItem item)
    {
        return new HistoryItemViewModel(this.navigationService, this.logger)
        {
            Id = item.Id,
            Title = item.Title,
            Subtitle = item.Subtitle,
            Details = item.Details ?? string.Empty,
            StartTime = $"{item.StartTime:yyyy-MM-dd HH:mm:ss}",
            EndTime = item.EndTime is not null ? $"{item.EndTime:yyyy-MM-dd HH:mm:ss}" : string.Empty,
            Status = item.Status switch
            {
                HistoryItemStatus.Pending => HistoryStatus.Pending,
                HistoryItemStatus.InProgress => HistoryStatus.InProgress,
                HistoryItemStatus.Completed => HistoryStatus.Completed,
                HistoryItemStatus.Canceled => HistoryStatus.Canceled,
                HistoryItemStatus.Error => HistoryStatus.Error,
                _ => HistoryStatus.Pending,
            },
            NavigationPage = item.NavigationPage,
            NavigationParameter = item.NavigationParameter,
        };
    }

    private void HistoryService_ItemUpdated(object? sender, HistoryItemEventArgs e)
    {
        this.dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                var existingViewModel = this.Items.FirstOrDefault(i => i.Id == e.Item.Id);
                if (existingViewModel is not null)
                {
                    var index = this.Items.IndexOf(existingViewModel);
                    if (index >= 0)
                    {
                        var updatedItemViewModel = this.GetHistoryItemViewModel(e.Item);
                        this.Items[index] = updatedItemViewModel;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to update history item {HistoryItemId}.", e.Item.Id);
            }
        });
    }

    private void HistoryService_ItemAdded(object? sender, HistoryItemEventArgs e)
    {
        this.dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                var historyItemViewModel = this.GetHistoryItemViewModel(e.Item);
                this.Items.Insert(0, historyItemViewModel);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to add history item {HistoryItemId}.", e.Item.Id);
            }
        });
    }

    private void HistoryService_ItemsReset(object? sender, EventArgs e)
    {
        this.dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                this.Items.Clear();
                this.PopulateHistory();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to reset history items.");
            }
        });
    }

    private void PopulateHistory()
    {
        foreach (var item in this.historyService.GetHistoryItems())
        {
            var historyItemViewModel = this.GetHistoryItemViewModel(item);
            this.Items.Insert(0, historyItemViewModel);
        }
    }

    [RelayCommand]
    private void Clear()
    {
        try
        {
            this.historyService.ClearHistory();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }
}
