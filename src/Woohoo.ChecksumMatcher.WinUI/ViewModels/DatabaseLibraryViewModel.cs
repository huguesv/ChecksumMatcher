// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.ViewModels;
using Woohoo.ChecksumMatcher.WinUI.Services;

public partial class DatabaseLibraryViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDatabaseService databaseService;
    private readonly IOfflineExplorerService offlineExplorerService;
    private readonly IClipboardService clipboardService;
    private readonly IFilePickerService filePickerService;
    private readonly IFileExplorerService fileExplorerService;
    private readonly IOperationCompletionService operationCompletionService;
    private readonly IDateTimeProviderService dateTimeProviderService;
    private readonly IDispatcherQueue dispatcherQueue;
    private readonly IDispatcherQueueTimer debounceTimer;

    public DatabaseLibraryViewModel(
        IDatabaseService databaseService,
        IOfflineExplorerService offlineExplorerService,
        IClipboardService clipboardService,
        IFilePickerService filePickerService,
        IDispatcherQueueService dispatcherQueueService,
        IFileExplorerService fileExplorerService,
        IOperationCompletionService operationCompletionService,
        IDateTimeProviderService dateTimeProviderService)
    {
        ArgumentNullException.ThrowIfNull(databaseService);
        ArgumentNullException.ThrowIfNull(offlineExplorerService);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(filePickerService);
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);
        ArgumentNullException.ThrowIfNull(operationCompletionService);
        ArgumentNullException.ThrowIfNull(dateTimeProviderService);

        this.databaseService = databaseService;
        this.offlineExplorerService = offlineExplorerService;
        this.clipboardService = clipboardService;
        this.filePickerService = filePickerService;
        this.fileExplorerService = fileExplorerService;
        this.operationCompletionService = operationCompletionService;
        this.dateTimeProviderService = dateTimeProviderService;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();
        this.debounceTimer = this.dispatcherQueue.CreateTimer();

        this.LoadDatabasesAsync(CancellationToken.None).SafeFireAndForget();

        this.databaseService.RepositoryChanged += this.DatabaseService_RepositoryChanged;
    }

    [ObservableProperty]
    public partial DatabaseTreeItemViewModel? Selected { get; set; }

    public ObservableCollection<DatabaseTreeItemViewModel> DataSource { get; } = [];

    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }

    private async Task LoadDatabasesAsync(CancellationToken ct)
    {
        var repository = await this.databaseService.GetRepositoryAsync(ct);
        foreach (var rootFolder in repository.RootFolders.OrderBy(f => f.Name))
        {
            var folderViewModel = await CreateFolderViewModelAsync(
                null,
                rootFolder,
                this.databaseService,
                this.offlineExplorerService,
                this.clipboardService,
                this.filePickerService,
                this.fileExplorerService,
                this.operationCompletionService,
                this.dateTimeProviderService,
                this.dispatcherQueue,
                ct);

            this.DataSource.Add(folderViewModel);
        }

        static async Task<DatabaseFolderTreeItemViewModel> CreateFolderViewModelAsync(
            DatabaseFolderTreeItemViewModel? parentFolderItemViewModel,
            DatabaseFolder folder,
            IDatabaseService databaseService,
            IOfflineExplorerService offlineExplorerService,
            IClipboardService clipboardService,
            IFilePickerService filePickerService,
            IFileExplorerService fileExplorerService,
            IOperationCompletionService operationCompletionService,
            IDateTimeProviderService dateTimeProviderService,
            IDispatcherQueue dispatcherQueue,
            CancellationToken ct)
        {
            var folderViewModel = await DatabaseFolderViewModel.CreateAsync(
                folder,
                databaseService,
                offlineExplorerService,
                clipboardService,
                filePickerService,
                operationCompletionService,
                dateTimeProviderService,
                dispatcherQueue,
                ct);

            var folderItemViewModel = new DatabaseFolderTreeItemViewModel(
                parentFolderItemViewModel,
                folder,
                folder.Name,
                Path.Combine(folder.RootAbsoluteFolderPath, folder.RelativePath),
                folderViewModel,
                fileExplorerService);

            foreach (var subFolder in folder.SubFolders.OrderBy(f => f.Name))
            {
                var subFolderItemViewModel = await CreateFolderViewModelAsync(
                    folderItemViewModel,
                    subFolder,
                    databaseService,
                    offlineExplorerService,
                    clipboardService,
                    filePickerService,
                    fileExplorerService,
                    operationCompletionService,
                    dateTimeProviderService,
                    dispatcherQueue,
                    ct);

                folderItemViewModel.Children.Add(subFolderItemViewModel);
            }

            foreach (var file in folder.Files.OrderBy(f => f.RelativePath))
            {
                var fileItemViewModel = new DatabaseFileTreeItemViewModel(
                    folderItemViewModel,
                    file,
                    string.Empty,
                    file.RootAbsoluteFolderPath,
                    file.FullPath,
                    file.RelativePath,
                    null,
                    databaseService,
                    fileExplorerService,
                    dispatcherQueue);

                folderItemViewModel.Children.Add(fileItemViewModel);
            }

            return folderItemViewModel;
        }
    }

    private void DatabaseService_RepositoryChanged(object? sender, EventArgs e)
    {
        this.debounceTimer.Debounce(DoReload, TimeSpan.FromMilliseconds(2000), immediate: false);

        void DoReload()
        {
            bool anyWorking = this.DataSource.Any(item => item is DatabaseFolderTreeItemViewModel folder && folder.Children.Any(child => child is DatabaseFileTreeItemViewModel file && (file.Database?.IsScanning == true || file.Database?.IsRebuilding == true)));
            if (!anyWorking)
            {
                this.DataSource.Clear();

                this.LoadDatabasesAsync(CancellationToken.None).SafeFireAndForget();
            }
            else
            {
                // TODO: Need to postpone until the work is completed, or notify user to restart later
            }
        }
    }

    partial void OnSelectedChanged(DatabaseTreeItemViewModel? value)
    {
        // Load the database contents on demand when the user selects it the first time.
        if (value is DatabaseFileTreeItemViewModel item && item.Database is null)
        {
            this.ReloadDatabaseItemAsync(item, CancellationToken.None).SafeFireAndForget();
        }
    }

    private async Task ReloadDatabaseItemAsync(DatabaseFileTreeItemViewModel item, CancellationToken ct)
    {
        var database = await this.databaseService.GetDatabaseAsync(item.DatabaseFile, ct);
        if (database is not null)
        {
            var parent = item.ParentFolderItem;
            var parentExpanded = parent.IsExpanded;

            var index = item.ParentFolderItem.Children.IndexOf(item);
            parent.Children.RemoveAt(index);

            var databaseViewModel = await DatabaseFileViewModel.CreateAsync(
                item.DatabaseFile,
                database,
                this.databaseService,
                this.offlineExplorerService,
                this.clipboardService,
                this.filePickerService,
                this.fileExplorerService,
                this.operationCompletionService,
                this.dateTimeProviderService,
                this.dispatcherQueue,
                ct);

            var databaseItemViewModel = item.WithDatabase(databaseViewModel);

            databaseItemViewModel.MatchedFiles = databaseViewModel.ScanMatchedFiles.Count;
            databaseItemViewModel.MissingFiles = databaseViewModel.ScanMissingFiles.Count;
            databaseItemViewModel.WrongNamedFiles = databaseViewModel.ScanIncorrectNameFiles.Count;
            databaseItemViewModel.UnusedFiles = databaseViewModel.ScanUnusedFiles.Count;

            parent.Children.Insert(index, databaseItemViewModel);
            parent.IsExpanded = parentExpanded;

            // Removing the old item above sets the Selected property to null.
            // Set it again unless user has already selected a different item.
            if (this.Selected is null || this.Selected == item)
            {
                this.Selected = databaseItemViewModel;
            }
        }
    }
}
