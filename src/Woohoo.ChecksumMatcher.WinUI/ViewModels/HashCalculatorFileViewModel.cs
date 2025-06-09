// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Globalization;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public sealed partial class HashCalculatorFileViewModel : ObservableObject
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
    public partial bool IsExpanded { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FolderPath))]
    [NotifyPropertyChangedFor(nameof(Name))]
    public partial string FullPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial long FileSize { get; set; }

    [ObservableProperty]
    public partial string Crc32 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Md5 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Sha1 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Sha256 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double DurationSecs { get; set; }

    public string Name => Path.GetFileName(this.FullPath) ?? string.Empty;

    public string FolderPath => Path.GetDirectoryName(this.FullPath) ?? string.Empty;

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
        current.AppendLine(string.Format(CultureInfo.CurrentUICulture, Localized.HashCalculatorPageResultCrc32, this.Crc32));
        current.AppendLine(string.Format(CultureInfo.CurrentUICulture, Localized.HashCalculatorPageResultMd5, this.Md5));
        current.AppendLine(string.Format(CultureInfo.CurrentUICulture, Localized.HashCalculatorPageResultSha1, this.Sha1));
        current.AppendLine(string.Format(CultureInfo.CurrentUICulture, Localized.HashCalculatorPageResultSha256, this.Sha256));

        this.clipboardService.SetText(current.ToString());
    }
}
