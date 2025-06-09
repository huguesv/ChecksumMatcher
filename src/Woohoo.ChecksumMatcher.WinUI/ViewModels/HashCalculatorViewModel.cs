// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.ViewModels;
using Woohoo.ChecksumMatcher.WinUI.Models;
using Woohoo.Security.Cryptography;

public partial class HashCalculatorViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService navigationService;
    private readonly IClipboardService clipboardService;
    private readonly IFilePickerService filePickerService;
    private readonly IFileExplorerService fileExplorerService;
    private readonly IDispatcherQueue dispatcherQueue;

    private CancellationTokenSource? cancellationTokenSource;

    public HashCalculatorViewModel(
        INavigationService navigationService,
        IClipboardService clipboardService,
        IFilePickerService filePickerService,
        IFileExplorerService fileExplorerService,
        IDispatcherQueueService dispatcherQueueService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(filePickerService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);

        this.navigationService = navigationService;
        this.clipboardService = clipboardService;
        this.filePickerService = filePickerService;
        this.fileExplorerService = fileExplorerService;
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

    [RelayCommand]
    private void Clear()
    {
        this.Results.Clear();
    }

    [RelayCommand(CanExecute = nameof(CanCancelCalculating))]
    private void CancelCalculating()
    {
        this.cancellationTokenSource?.Cancel();
        this.CancelCalculatingCommand.NotifyCanExecuteChanged();
    }

    private bool CanCancelCalculating()
    {
        return this.IsCalculating && this.cancellationTokenSource?.IsCancellationRequested == false;
    }

    [RelayCommand(CanExecute = nameof(CanSelectFile))]
    private async Task SelectFileAsync()
    {
        var filePath = await this.filePickerService.GetOpenFilePathAsync(FilePickerSettingIdentifiers.HashFile);
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        this.cancellationTokenSource = new CancellationTokenSource();
        this.IsCalculating = true;

        var item = StartItem(filePath, this.clipboardService, this.fileExplorerService);
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
            return calculator.Calculate(["crc32", "md5", "sha1", "sha256"], filePath);

            void Calculator_Progress(object? sender, HashCalculatorProgressEventArgs e)
            {
                ct.ThrowIfCancellationRequested();

                this.dispatcherQueue.TryEnqueue(() => item.FileProgress = e.ProgressPercentage);
            }
        }

        static HashCalculatorFileViewModel StartItem(string filePath, IClipboardService clipboardService, IFileExplorerService fileExplorerService)
        {
            var info = new FileInfo(filePath);

            var item = new HashCalculatorFileViewModel(clipboardService, fileExplorerService)
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
            item.Hashes.Add(new HashCalculatorChecksumViewModel() { Algorithm = Localized.HashCalculatorPageFileCrc32, Value = HashCalculator.HexToString(result.Checksums["crc32"]) });
            item.Hashes.Add(new HashCalculatorChecksumViewModel() { Algorithm = Localized.HashCalculatorPageFileMd5, Value = HashCalculator.HexToString(result.Checksums["md5"]) });
            item.Hashes.Add(new HashCalculatorChecksumViewModel() { Algorithm = Localized.HashCalculatorPageFileSha1, Value = HashCalculator.HexToString(result.Checksums["sha1"]) });
            item.Hashes.Add(new HashCalculatorChecksumViewModel() { Algorithm = Localized.HashCalculatorPageFileSha256, Value = HashCalculator.HexToString(result.Checksums["sha256"]) });
        }
    }

    private bool CanSelectFile() => !this.IsCalculating;
}
