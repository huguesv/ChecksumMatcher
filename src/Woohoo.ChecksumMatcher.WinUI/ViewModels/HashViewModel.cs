// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.System;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.ViewModels;
using Woohoo.Security.Cryptography;

public partial class HashViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService navigationService;
    private readonly IClipboardService clipboardService;
    private readonly IFilePickerService filePickerService;
    private readonly DispatcherQueue dispatcherQueue;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SelectFileCommand))]
    private bool isCalculating;

    public ObservableCollection<HashFileResultViewModel> Results { get; set; } = [];

    public HashViewModel(INavigationService navigationService, IClipboardService clipboardService, IFilePickerService filePickerService, IDispatcherQueueService dispatcherQueueService)
    {
        this.navigationService = navigationService;
        this.clipboardService = clipboardService;
        this.filePickerService = filePickerService;
        this.dispatcherQueue = dispatcherQueueService.GetWindowsDispatcher();
    }

    public void OnNavigatedTo(object parameter)
    {
        //Source.Clear();

        //// TODO: Replace with real data.
        //var data = await _sampleDataService.GetContentGridDataAsync();
        //foreach (var item in data)
        //{
        //    Source.Add(item);
        //}
    }

    public void OnNavigatedFrom()
    {
    }

    //[RelayCommand]
    //private void OnItemClick(SampleOrder? clickedItem)
    //{
    //    if (clickedItem != null)
    //    {
    //        //_navigationService.SetListDataItemForNextConnectedAnimation(clickedItem);
    //        //_navigationService.NavigateTo(typeof(ContentGridDetailViewModel).FullName!, clickedItem.OrderID);
    //    }
    //}

    [RelayCommand(CanExecute = nameof(CanSelectFile))]
    public async Task SelectFileAsync()
    {
        var filePath = await this.filePickerService.GetOpenFilePathAsync();
        if (!string.IsNullOrEmpty(filePath))
        {
            this.IsCalculating = true;

            var item = this.StartItem(filePath);

            _ = Task.Run(() => this.ComputeChecksums(filePath, item));
        }
    }

    public bool CanSelectFile() => !this.IsCalculating;

    private void ComputeChecksums(string filePath, HashFileResultViewModel item)
    {
        var calculator = new HashCalculator();
        calculator.Progress += Calculator_Progress;
        var result = calculator.Calculate(["crc32", "md5", "sha1", "sha256"], filePath);
        this.dispatcherQueue.TryEnqueue(() =>
        {
            this.CompleteItem(item, result);
            this.IsCalculating = false;
        });

        void Calculator_Progress(object? sender, HashCalculatorProgressEventArgs e)
        {
            if (this.dispatcherQueue.HasThreadAccess)
            {
                item.FileProgress = e.ProgressPercentage;
            }
            else
            {
                this.dispatcherQueue.TryEnqueue(() => item.FileProgress = e.ProgressPercentage);
            }
        }
    }

    [RelayCommand]
    public void CollapseAll()
    {
        foreach (var item in this.Results)
        {
            item.IsExpanded = false;
        }
    }

    [RelayCommand]
    public void Clear()
    {
        this.Results.Clear();
    }

    private void CompleteItem(HashFileResultViewModel item, HashCalculatorResult result)
    {
        item.IsCalculating = false;
        item.Hashes.Add(new HashChecksumResultViewModel() { Algorithm = "CRC32", Value = HashCalculator.HexToString(result.Checksums["crc32"]) });
        item.Hashes.Add(new HashChecksumResultViewModel() { Algorithm = "MD5", Value = HashCalculator.HexToString(result.Checksums["md5"]) });
        item.Hashes.Add(new HashChecksumResultViewModel() { Algorithm = "SHA1", Value = HashCalculator.HexToString(result.Checksums["sha1"]) });
        item.Hashes.Add(new HashChecksumResultViewModel() { Algorithm = "SHA256", Value = HashCalculator.HexToString(result.Checksums["sha256"]) });
    }

    private HashFileResultViewModel StartItem(string filePath)
    {
        var info = new FileInfo(filePath);

        var item = new HashFileResultViewModel(this.clipboardService)
        {
            FullPath = filePath,
            FileSize = info.Length,
            Id = Guid.NewGuid(),
            IsCalculating = true,
        };

        this.Results.Add(item);
        return item;
    }
}
