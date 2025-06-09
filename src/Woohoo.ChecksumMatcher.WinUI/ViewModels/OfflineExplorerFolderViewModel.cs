// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Collections;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Helpers;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

[DebuggerDisplay("Name = {Name}")]
public sealed partial class OfflineExplorerFolderViewModel : ObservableObject
{
    private readonly IDispatcherQueueTimer filterTextDebounceTimer;
    private readonly OfflineItem item;
    private readonly OfflineExplorerViewModel offlineExplorerViewModel;
    private readonly ILocalSettingsService localSettingsService;
    private readonly IClipboardService clipboardService;

    private long? filterTextNumeric = null;

    public OfflineExplorerFolderViewModel(
        OfflineItem item,
        OfflineExplorerViewModel offlineExplorerViewModel,
        OfflineExplorerFolderViewModel? parentViewModel,
        ILocalSettingsService localSettingsService,
        IClipboardService clipboardService,
        IDispatcherQueue dispatcherQueue)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(offlineExplorerViewModel);
        ArgumentNullException.ThrowIfNull(localSettingsService);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(dispatcherQueue);

        this.filterTextDebounceTimer = dispatcherQueue.CreateTimer();
        this.item = item;
        this.offlineExplorerViewModel = offlineExplorerViewModel;
        this.ParentViewModel = parentViewModel;
        this.localSettingsService = localSettingsService;
        this.clipboardService = clipboardService;
        this.Breadcrumbs = CreateBreadcrumbs(this);
        this.SortedFolders = new AdvancedCollectionView(this.Folders, true);
        this.SortedFoldersAndFiles = new AdvancedCollectionView(this.FoldersAndFiles, true);
        this.SetSortDescriptions();

        var showArchivesInTree = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.ExplorerShowArchiveInTree) ?? false;
        if (!showArchivesInTree)
        {
            this.SortedFolders.Filter = x => x is OfflineExplorerFolderViewModel f && f.Kind == OfflineItemKind.Folder;
            this.SortedFolders.RefreshFilter();
        }
    }

    [ObservableProperty]
    public partial string FilterText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool SearchResultsVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsExpanded { get; set; } = false;

    [ObservableProperty]
    public partial object? SelectedFile { get; set; }

    [ObservableProperty]
    public partial bool SelectedFileDetailsAvailable { get; set; }

    public AdvancedCollectionView SortedFolders { get; }

    public ObservableCollection<OfflineExplorerFolderViewModel> Folders { get; private set; } = [];

    public ObservableCollection<OfflineExplorerFileViewModel> Files { get; private set; } = [];

    public AdvancedCollectionView SortedFoldersAndFiles { get; }

    public ObservableCollection<object> FoldersAndFiles { get; private set; } = [];

    public ObservableCollection<OfflineExplorerSearchResultViewModel> SearchResults { get; private set; } = [];

    public OfflineExplorerBreadcrumbViewModel[] Breadcrumbs { get; }

    public string Name => this.item.Name;

    public string Path => this.item.Path;

    public long Size => this.FoldersAndFiles.Sum(x => x is OfflineExplorerFileViewModel fil ? fil.Size : x is OfflineExplorerFolderViewModel fol ? fol.Size : 0);

    public string ReportedCRC32 => this.item.ReportedCRC32 ?? string.Empty;

    public string CRC32 => this.item.CRC32 ?? string.Empty;

    public string MD5 => this.item.MD5 ?? string.Empty;

    public string SHA1 => this.item.SHA1 ?? string.Empty;

    public string SHA256 => this.item.SHA256 ?? string.Empty;

    public string SHA512 => this.item.SHA512 ?? string.Empty;

    public string Created => this.item.Created?.ToString() ?? string.Empty;

    public string Modified => this.item.Modified?.ToString() ?? string.Empty;

    public OfflineItemKind Kind => this.item.Kind;

    public OfflineExplorerFolderViewModel? ParentViewModel { get; }

    public void TraverseFiles(Action<OfflineExplorerFileViewModel> action)
    {
        foreach (var item in this.Files)
        {
            action(item);
        }

        foreach (var folder in this.Folders)
        {
            folder.TraverseFiles(action);
        }
    }

    [RelayCommand]
    public void CollapseAllDescendants()
    {
        foreach (var child in this.Folders)
        {
            child.CollapseAllDescendants();
        }

        this.IsExpanded = false;
    }

    [RelayCommand]
    public void ExpandAllDescendants()
    {
        foreach (var child in this.Folders)
        {
            child.ExpandAllDescendants();
        }

        this.IsExpanded = true;
    }

    [RelayCommand]
    public void CopyToClipboard(object value)
    {
        this.clipboardService.SetText(value.ToString() ?? string.Empty);
    }

    private static OfflineExplorerBreadcrumbViewModel[] CreateBreadcrumbs(OfflineExplorerFolderViewModel item)
    {
        List<OfflineExplorerBreadcrumbViewModel> breadcrumbs = [];

        OfflineExplorerFolderViewModel? currentItem = item;
        while (currentItem is not null)
        {
            breadcrumbs.Add(new OfflineExplorerBreadcrumbViewModel(currentItem));
            currentItem = currentItem.ParentViewModel;
        }

        breadcrumbs.Reverse();

        return [.. breadcrumbs];
    }

    private void SetSortDescriptions()
    {
        var direction = Enum.Parse<SortDirection>(this.localSettingsService.ReadSetting<string>(KnownSettingKeys.ExplorerSortOrder) ?? SortDirection.Ascending.ToString());
        var priority = Enum.Parse<OfflineExplorerFolderSortPriority>(this.localSettingsService.ReadSetting<string>(KnownSettingKeys.ExplorerFolderSortPriority) ?? OfflineExplorerFolderSortPriority.First.ToString());
        var archiveGrouping = Enum.Parse<OfflineExplorerArchiveGrouping>(this.localSettingsService.ReadSetting<string>(KnownSettingKeys.ExplorerArchiveGrouping) ?? OfflineExplorerArchiveGrouping.WithFiles.ToString());

        this.SortedFoldersAndFiles.SortDescriptions.Clear();
        this.SortedFoldersAndFiles.SortDescriptions.Add(new SortDescription(direction, new FolderAndFileComparer(priority, archiveGrouping)));

        this.SortedFolders.SortDescriptions.Clear();
        this.SortedFolders.SortDescriptions.Add(new SortDescription(direction, new FolderAndFileComparer(priority, archiveGrouping)));
    }

    partial void OnSelectedFileChanged(object? value)
    {
        this.SelectedFileDetailsAvailable = value is not null;
    }

    partial void OnFilterTextChanged(string value)
    {
        this.filterTextDebounceTimer.Debounce(() =>
        {

            if (long.TryParse(value, out long result))
            {
                this.filterTextNumeric = result;
            }
            else
            {
                this.filterTextNumeric = null;
            }

            this.SearchResults.Clear();

            if (string.IsNullOrWhiteSpace(value))
            {
                this.SearchResultsVisible = false;
            }
            else
            {
                this.TraverseFiles(file =>
                {
                    if (file.Match(value, this.filterTextNumeric))
                    {
                        this.SearchResults.Add(new OfflineExplorerSearchResultViewModel(file, this.offlineExplorerViewModel));
                    }
                });

                this.SearchResultsVisible = true;
            }
        }, TimeSpan.FromMilliseconds(500), immediate: string.IsNullOrWhiteSpace(value));
    }

    private class FolderAndFileComparer : System.Collections.IComparer
    {
        private readonly OfflineExplorerFolderSortPriority folderSortPriority;
        private readonly OfflineExplorerArchiveGrouping archiveGrouping;

        public FolderAndFileComparer(OfflineExplorerFolderSortPriority folderSortPriority, OfflineExplorerArchiveGrouping archiveGrouping)
        {
            this.folderSortPriority = folderSortPriority;
            this.archiveGrouping = archiveGrouping;
        }

        public int Compare(object? x, object? y)
        {
            if (x is OfflineExplorerFolderViewModel folderX)
            {
                if (folderX.Kind == OfflineItemKind.Folder || this.archiveGrouping == OfflineExplorerArchiveGrouping.WithFolders)
                {
                    return this.CompareLogicalFolderWith(folderX.Name, y);
                }
                else if (folderX.Kind == OfflineItemKind.ArchiveFile)
                {
                    return this.CompareLogicalFileWith(folderX.Name, y);
                }
                else
                {
                    throw new NotSupportedException("Comparison not supported for the provided types.");
                }
            }
            else if (x is OfflineExplorerFileViewModel fileX)
            {
                return this.CompareLogicalFileWith(fileX.Name, y);
            }
            else
            {
                throw new NotSupportedException("Comparison not supported for the provided types.");
            }
        }

        private int CompareLogicalFileWith(string fileName, object? y)
        {
            if (y is OfflineExplorerFolderViewModel folderY)
            {
                if (folderY.Kind == OfflineItemKind.Folder || this.archiveGrouping == OfflineExplorerArchiveGrouping.WithFolders)
                {
                    if (this.folderSortPriority == OfflineExplorerFolderSortPriority.First)
                    {
                        return 1; // Files come after folders
                    }
                    else if (this.folderSortPriority == OfflineExplorerFolderSortPriority.Last)
                    {
                        return -1; // Files come before folders
                    }
                    else
                    {
                        return string.Compare(fileName, folderY.Name, StringComparison.OrdinalIgnoreCase);
                    }
                }
                else
                {
                    return string.Compare(fileName, folderY.Name, StringComparison.OrdinalIgnoreCase);
                }
            }
            else if (y is OfflineExplorerFileViewModel fileY)
            {
                return string.Compare(fileName, fileY.Name, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                throw new NotSupportedException("Comparison not supported for the provided types.");
            }
        }

        private int CompareLogicalFolderWith(string folderName, object? y)
        {
            if (y is OfflineExplorerFolderViewModel folderY)
            {
                return string.Compare(folderName, folderY.Name, StringComparison.OrdinalIgnoreCase);
            }
            else if (y is OfflineExplorerFileViewModel fileY)
            {
                if (this.folderSortPriority == OfflineExplorerFolderSortPriority.First)
                {
                    return -1; // Folders come before files
                }
                else if (this.folderSortPriority == OfflineExplorerFolderSortPriority.Last)
                {
                    return 1; // Folders come after files
                }
                else
                {
                    return string.Compare(folderName, fileY.Name, StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                throw new NotSupportedException("Comparison not supported for the provided types.");
            }
        }
    }
}
