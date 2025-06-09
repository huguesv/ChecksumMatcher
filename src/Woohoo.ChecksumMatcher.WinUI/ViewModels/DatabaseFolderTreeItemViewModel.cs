// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public partial class DatabaseFolderTreeItemViewModel : DatabaseTreeItemViewModel
{
    private readonly string folderPath;
    private readonly IFileExplorerService fileExplorerService;

    public DatabaseFolderTreeItemViewModel(
        DatabaseFolderTreeItemViewModel? parentFolderItem,
        DatabaseFolder databaseFolder,
        string name,
        string folderPath,
        DatabaseFolderViewModel folderViewModel,
        IFileExplorerService fileExplorerService)
        : base(name, ExplorerItemType.Folder)
    {
        ArgumentNullException.ThrowIfNull(databaseFolder);
        ArgumentNullException.ThrowIfNull(folderPath);
        ArgumentNullException.ThrowIfNull(folderViewModel);
        ArgumentNullException.ThrowIfNull(fileExplorerService);

        this.ParentFolderItem = parentFolderItem;
        this.DatabaseFolder = databaseFolder;
        this.folderPath = folderPath;
        this.FolderViewModel = folderViewModel;
        this.fileExplorerService = fileExplorerService;
    }

    public DatabaseFolderTreeItemViewModel? ParentFolderItem { get; }

    public DatabaseFolder DatabaseFolder { get; }

    public DatabaseFolderViewModel FolderViewModel { get; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [RelayCommand]
    private void OpenInExplorer()
    {
        this.fileExplorerService.OpenInExplorer(this.folderPath);
    }
}
