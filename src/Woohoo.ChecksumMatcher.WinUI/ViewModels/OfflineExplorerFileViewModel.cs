// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

[DebuggerDisplay("Name = {Name}")]
public sealed partial class OfflineExplorerFileViewModel : ObservableObject
{
    private readonly OfflineItem item;
    private readonly IClipboardService clipboardService;

    public OfflineExplorerFileViewModel(
        OfflineItem item,
        OfflineExplorerFolderViewModel? parentViewModel,
        IClipboardService clipboardService)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(clipboardService);

        this.item = item;
        this.ParentViewModel = parentViewModel;
        this.clipboardService = clipboardService;
    }

    public string Name => this.item.Name;

    public string Path => this.item.Path;

    public string Container => this.item.ParentItem?.Path ?? string.Empty;

    public long Size => this.item.Size ?? 0;

    public string ReportedCRC32 => this.item.ReportedCRC32 ?? string.Empty;

    public string CRC32 => this.item.CRC32 ?? string.Empty;

    public string MD5 => this.item.MD5 ?? string.Empty;

    public string SHA1 => this.item.SHA1 ?? string.Empty;

    public string SHA256 => this.item.SHA256 ?? string.Empty;

    public string SHA512 => this.item.SHA512 ?? string.Empty;

    public string Created => this.item.Created?.ToString() ?? string.Empty;

    public string Modified => this.item.Modified?.ToString() ?? string.Empty;

    public OfflineExplorerFolderViewModel? ParentViewModel { get; }

    public bool Match(string searchTerm, long? searchTermNumeric)
    {
        return this.item.Match(searchTerm, searchTermNumeric);
    }

    [RelayCommand]
    private void CopyToClipboard(object value)
    {
        this.clipboardService.SetText(value.ToString() ?? string.Empty);
    }
}
