// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public partial class DatabaseFileTreeItemViewModel : DatabaseTreeItemViewModel
{
    private readonly IDatabaseService databaseService;
    private readonly IFileExplorerService fileExplorerService;
    private readonly IDispatcherQueue dispatcherQueue;

    public DatabaseFileTreeItemViewModel(
        DatabaseFolderTreeItemViewModel parentFolderItem,
        DatabaseFile databaseFile,
        string name,
        string rootFolder,
        string absoluteFilePath,
        string relativeFilePath,
        DatabaseFileViewModel? databaseViewModel,
        IDatabaseService databaseService,
        IFileExplorerService fileExplorerService,
        IDispatcherQueue dispatcherQueue)
        : base(name, ExplorerItemType.File)
    {
        ArgumentNullException.ThrowIfNull(parentFolderItem);
        ArgumentNullException.ThrowIfNull(databaseFile);
        ArgumentNullException.ThrowIfNull(rootFolder);
        ArgumentNullException.ThrowIfNull(absoluteFilePath);
        ArgumentNullException.ThrowIfNull(databaseService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);
        ArgumentNullException.ThrowIfNull(dispatcherQueue);

        this.ParentFolderItem = parentFolderItem;
        this.DatabaseFile = databaseFile;
        this.RootFolder = rootFolder;
        this.AbsoluteFilePath = absoluteFilePath;
        this.RelativeFilePath = relativeFilePath;
        this.FileName = Path.GetFileNameWithoutExtension(absoluteFilePath);
        this.Database = databaseViewModel;
        this.databaseService = databaseService;
        this.fileExplorerService = fileExplorerService;
        this.dispatcherQueue = dispatcherQueue;

        this.databaseService.ScanProgress += this.DatabaseService_ScanProgress;
    }

    [ObservableProperty]
    public partial int MatchedFiles { get; set; }

    [ObservableProperty]
    public partial int MissingFiles { get; set; }

    [ObservableProperty]
    public partial int WrongNamedFiles { get; set; }

    [ObservableProperty]
    public partial int UnusedFiles { get; set; }

    public DatabaseFolderTreeItemViewModel ParentFolderItem { get; }

    public DatabaseFile DatabaseFile { get; }

    public string RootFolder { get; }

    public string AbsoluteFilePath { get; }

    public string RelativeFilePath { get; }

    public string FileName { get; }

    public DatabaseFileViewModel? Database { get; }

    public DatabaseFileTreeItemViewModel WithDatabase(DatabaseFileViewModel databaseViewModel)
    {
        var result = new DatabaseFileTreeItemViewModel(
            this.ParentFolderItem,
            this.DatabaseFile,
            this.Name,
            this.RootFolder,
            this.AbsoluteFilePath,
            this.RelativeFilePath,
            databaseViewModel,
            this.databaseService,
            this.fileExplorerService,
            this.dispatcherQueue);

        result.MatchedFiles = this.MatchedFiles;
        result.MissingFiles = this.MissingFiles;
        result.WrongNamedFiles = this.WrongNamedFiles;
        result.UnusedFiles = this.UnusedFiles;

        return result;
    }

    [RelayCommand]
    private void OpenInExplorer()
    {
        this.fileExplorerService.OpenInExplorer(this.AbsoluteFilePath);
    }

    private void DatabaseService_ScanProgress(object? sender, ScanEventArgs e)
    {
        if (e.DatabaseFile.FullPath != this.DatabaseFile.FullPath)
        {
            return;
        }

        this.dispatcherQueue.TryEnqueue(() =>
        {
            switch (e.Status)
            {
                case ScanStatus.Cleared:
                    this.MatchedFiles = 0;
                    this.MissingFiles = 0;
                    this.WrongNamedFiles = 0;
                    this.UnusedFiles = 0;
                    break;
                default:
                    break;
            }

            this.MatchedFiles += e.Results.Matched.Length;
            this.MissingFiles += e.Results.Missing.Length;
            this.WrongNamedFiles += e.Results.WrongNamed.Length;
            this.UnusedFiles += e.Results.Unused.Length;
        });
    }
}
