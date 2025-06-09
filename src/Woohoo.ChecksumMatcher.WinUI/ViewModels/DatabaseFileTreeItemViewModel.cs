// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Helpers;

[DebuggerDisplay("Name = {Name} Path = {AbsoluteFilePath}")]
public sealed partial class DatabaseFileTreeItemViewModel : DatabaseTreeItemViewModel, IDisposable
{
    private readonly DisposableBag disposables = DisposableBag.Create<DatabaseLibraryViewModel>();

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
        : base(name, ExplorerItemType.File, parentFolderItem)
    {
        ArgumentNullException.ThrowIfNull(parentFolderItem);
        ArgumentNullException.ThrowIfNull(databaseFile);
        ArgumentNullException.ThrowIfNull(rootFolder);
        ArgumentNullException.ThrowIfNull(absoluteFilePath);
        ArgumentNullException.ThrowIfNull(databaseService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);
        ArgumentNullException.ThrowIfNull(dispatcherQueue);

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
        this.disposables.Add(() => this.databaseService.ScanProgress -= this.DatabaseService_ScanProgress);
    }

    [ObservableProperty]
    public partial int MatchedFilesCount { get; set; }

    [ObservableProperty]
    public partial int MissingFilesCount { get; set; }

    [ObservableProperty]
    public partial int WrongNamedFilesCount { get; set; }

    [ObservableProperty]
    public partial int UnusedFilesCount { get; set; }

    [ObservableProperty]
    public partial bool HasScanResults { get; set; }

    public List<RomMoniker> MissingFiles { get; } = [];

    public DatabaseFile DatabaseFile { get; }

    public string RootFolder { get; }

    public string AbsoluteFilePath { get; }

    public string RelativeFilePath { get; }

    public string FileName { get; }

    public DatabaseFileViewModel? Database { get; }

    public void Dispose()
    {
        this.disposables.TryDispose();
    }

    public DatabaseFileTreeItemViewModel WithDatabase(DatabaseFileViewModel databaseViewModel)
    {
        if (this.ParentFolderItem is null)
        {
            throw new InvalidOperationException("ParentFolderItem cannot be null.");
        }

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
            this.dispatcherQueue)
        {
            MatchedFilesCount = this.MatchedFilesCount,
            MissingFilesCount = this.MissingFilesCount,
            WrongNamedFilesCount = this.WrongNamedFilesCount,
            UnusedFilesCount = this.UnusedFilesCount,
        };

        return result;
    }

    protected override bool IsFilterNameMatch()
        => base.IsFilterNameMatch() ||
           this.FileName.Contains(this.FilterText, StringComparison.OrdinalIgnoreCase);

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
                    this.HasScanResults = false;
                    this.MissingFiles.Clear();
                    this.MatchedFilesCount = 0;
                    this.MissingFilesCount = 0;
                    this.WrongNamedFilesCount = 0;
                    this.UnusedFilesCount = 0;
                    break;
                case ScanStatus.Completed:
                    this.HasScanResults = true;
                    break;
                default:
                    break;
            }

            this.MissingFiles.AddRange(e.Results.Missing);
            this.MatchedFilesCount += e.Results.Matched.Length;
            this.MissingFilesCount += e.Results.Missing.Length;
            this.WrongNamedFilesCount += e.Results.WrongNamed.Length;
            this.UnusedFilesCount += e.Results.Unused.Length;
        });
    }
}
