// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public partial class HashFileResultViewModel : ObservableObject
{
    private readonly IClipboardService clipboardService;
    [ObservableProperty]
    private bool isCalculating = false;

    [ObservableProperty]
    private bool isExpanded = true;

    [ObservableProperty]
    private int fileProgress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FolderPath))]
    [NotifyPropertyChangedFor(nameof(Name))]
    private string fullPath = string.Empty;

    [ObservableProperty]
    private long fileSize = 0;

    [ObservableProperty]
    private double durationSecs = 0;

    public HashFileResultViewModel(IClipboardService clipboardService)
    {
        this.clipboardService = clipboardService;
    }

    public string Name => Path.GetFileName(this.FullPath) ?? string.Empty;

    public string FolderPath => Path.GetDirectoryName(this.FullPath) ?? string.Empty;

    public ObservableCollection<HashChecksumResultViewModel> Hashes { get; set; } = new();

    public Guid Id { get; init; }

    [RelayCommand]
    public void CopyHash()
    {
        var current = new StringBuilder();

        current.AppendLine($"File: {this.FullPath}");
        current.AppendLine($"Size: {this.FileSize}");
        foreach (var hash in this.Hashes)
        {
            current.AppendLine($"{hash.Algorithm}: {hash.Value}");
        }

        this.clipboardService.SetText(current.ToString());
    }
}
