// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public partial class HashCalculatorFileViewModel : ObservableObject
{
    private readonly IClipboardService clipboardService;
    private readonly IFileExplorerService fileExplorerService;

    public HashCalculatorFileViewModel(IClipboardService clipboardService, IFileExplorerService fileExplorerService)
    {
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);

        this.clipboardService = clipboardService;
        this.fileExplorerService = fileExplorerService;
    }

    [ObservableProperty]
    public partial bool IsCalculating { get; set; } = false;

    [ObservableProperty]
    public partial bool IsCalculatingError { get; set; }

    [ObservableProperty]
    public partial int FileProgress { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FolderPath))]
    [NotifyPropertyChangedFor(nameof(Name))]
    public partial string FullPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial long FileSize { get; set; }

    [ObservableProperty]
    public partial double DurationSecs { get; set; }

    public string Name => Path.GetFileName(this.FullPath) ?? string.Empty;

    public string FolderPath => Path.GetDirectoryName(this.FullPath) ?? string.Empty;

    public ObservableCollection<HashCalculatorChecksumViewModel> Hashes { get; set; } = [];

    [RelayCommand]
    private void OpenInExplorer()
    {
        this.fileExplorerService.OpenInExplorer(this.FullPath);
    }

    [RelayCommand]
    private void CopyHash()
    {
        var current = new StringBuilder();

        current.AppendLine(string.Format(CultureInfo.CurrentUICulture, Localized.HashCalculatorPageResultFileName, this.Name));
        current.AppendLine(string.Format(CultureInfo.CurrentUICulture, Localized.HashCalculatorPageResultFileSize, this.FileSize));

        foreach (var hash in this.Hashes)
        {
            current.AppendLine(string.Format(CultureInfo.CurrentUICulture, Localized.HashCalculatorPageResultHashNameAndValue, hash.Algorithm, hash.Value));
        }

        this.clipboardService.SetText(current.ToString());
    }
}
