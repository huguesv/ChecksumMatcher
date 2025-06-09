// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.ViewModels;
using Woohoo.ChecksumMatcher.WinUI.Models;
using Woohoo.Security.Cryptography;

public sealed partial class HashCalculatorViewModel : ObservableRecipient, INavigationAware
{
    private readonly IClipboardService clipboardService;
    private readonly IFilePickerService filePickerService;
    private readonly IFileExplorerService fileExplorerService;
    private readonly ILogger logger;
    private readonly IDispatcherQueue dispatcherQueue;

    private CancellationTokenSource? cancellationTokenSource;

    public HashCalculatorViewModel(
        IClipboardService clipboardService,
        IFilePickerService filePickerService,
        IFileExplorerService fileExplorerService,
        IDispatcherQueueService dispatcherQueueService,
        ILogger<HashCalculatorViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(filePickerService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);
        ArgumentNullException.ThrowIfNull(logger);

        this.clipboardService = clipboardService;
        this.filePickerService = filePickerService;
        this.fileExplorerService = fileExplorerService;
        this.logger = logger;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SelectFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCalculatingCommand))]
    public partial bool IsCalculating { get; set; }

    public ObservableCollection<HashCalculatorFileViewModel> Results { get; set; } = [];

    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }

    internal async Task ProcessFileAsync(string filePath)
    {
        this.cancellationTokenSource = new CancellationTokenSource();
        this.IsCalculating = true;

        var item = StartItem(filePath, this.clipboardService, this.fileExplorerService, this.logger);
        try
        {
            this.Results.Add(item);

            var result = await Task.Run(() => ComputeChecksums(filePath, item, this.cancellationTokenSource.Token));
            CompleteItem(item, result);
        }
        catch (OperationCanceledException)
        {
            item.IsCalculatingError = true;
        }
        finally
        {
            this.IsCalculating = false;
            this.cancellationTokenSource?.Dispose();
            this.cancellationTokenSource = null;
        }

        HashCalculatorResult ComputeChecksums(string filePath, HashCalculatorFileViewModel item, CancellationToken ct)
        {
            var calculator = new HashCalculator();
            calculator.Progress += Calculator_Progress;
            try
            {
                return calculator.Calculate(["crc32", "md5", "sha1", "sha256"], filePath);
            }
            finally
            {
                calculator.Progress -= Calculator_Progress;
            }

            void Calculator_Progress(object? sender, HashCalculatorProgressEventArgs e)
            {
                ct.ThrowIfCancellationRequested();

                this.dispatcherQueue.TryEnqueue(() => item.FileProgress = e.ProgressPercentage);
            }
        }

        static HashCalculatorFileViewModel StartItem(string filePath, IClipboardService clipboardService, IFileExplorerService fileExplorerService, ILogger logger)
        {
            var info = new FileInfo(filePath);

            var item = new HashCalculatorFileViewModel(clipboardService, fileExplorerService, logger)
            {
                FullPath = filePath,
                FileSize = info.Length,
                IsCalculating = true,
            };

            return item;
        }

        static void CompleteItem(HashCalculatorFileViewModel item, HashCalculatorResult result)
        {
            item.IsCalculating = false;
            item.Crc32 = HashCalculator.HexToString(result.Checksums["crc32"]);
            item.Md5 = HashCalculator.HexToString(result.Checksums["md5"]);
            item.Sha1 = HashCalculator.HexToString(result.Checksums["sha1"]);
            item.Sha256 = HashCalculator.HexToString(result.Checksums["sha256"]);
        }
    }

    [RelayCommand]
    private void Clear()
    {
        try
        {
            this.Results.Clear();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private void CollapseAll()
    {
        try
        {
            foreach (var item in this.Results)
            {
                item.IsExpanded = false;
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelCalculating))]
    private void CancelCalculating()
    {
        try
        {
            this.cancellationTokenSource?.Cancel();
            this.CancelCalculatingCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    private bool CanCancelCalculating()
    {
        return this.IsCalculating && this.cancellationTokenSource?.IsCancellationRequested == false;
    }

    [RelayCommand(CanExecute = nameof(CanSelectFile))]
    private async Task SelectFileAsync()
    {
        try
        {
            var filePath = await this.filePickerService.GetOpenFilePathAsync(FilePickerSettingIdentifiers.HashFile);
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            await this.ProcessFileAsync(filePath);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    private bool CanSelectFile() => !this.IsCalculating;
}
