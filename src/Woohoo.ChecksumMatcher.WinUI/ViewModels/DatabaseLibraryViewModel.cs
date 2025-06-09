// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.ViewModels;
using Woohoo.ChecksumMatcher.WinUI.Helpers;

public sealed partial class DatabaseLibraryViewModel : ObservableRecipient, INavigationAware, IDisposable
{
    private readonly DisposableBag disposables = DisposableBag.Create<DatabaseLibraryViewModel>();

    private readonly IDatabaseService databaseService;
    private readonly IClipboardService clipboardService;
    private readonly IFilePickerService filePickerService;
    private readonly IFileExplorerService fileExplorerService;
    private readonly IOperationCompletionService operationCompletionService;
    private readonly IDateTimeProviderService dateTimeProviderService;
    private readonly INavigationService navigationService;
    private readonly IRestartService restartService;
    private readonly ILogger logger;
    private readonly IDispatcherQueue dispatcherQueue;
    private readonly IDispatcherQueueTimer repositoryChangeDebounceTimer;
    private readonly IDispatcherQueueTimer filterTextDebounceTimer;

    public DatabaseLibraryViewModel(
        IDatabaseService databaseService,
        IClipboardService clipboardService,
        IFilePickerService filePickerService,
        IDispatcherQueueService dispatcherQueueService,
        IFileExplorerService fileExplorerService,
        IOperationCompletionService operationCompletionService,
        IDateTimeProviderService dateTimeProviderService,
        INavigationService navigationService,
        IRestartService restartService,
        ILogger<DatabaseLibraryViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(databaseService);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(filePickerService);
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);
        ArgumentNullException.ThrowIfNull(operationCompletionService);
        ArgumentNullException.ThrowIfNull(dateTimeProviderService);
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(restartService);
        ArgumentNullException.ThrowIfNull(logger);

        this.databaseService = databaseService;
        this.clipboardService = clipboardService;
        this.filePickerService = filePickerService;
        this.fileExplorerService = fileExplorerService;
        this.operationCompletionService = operationCompletionService;
        this.dateTimeProviderService = dateTimeProviderService;
        this.navigationService = navigationService;
        this.restartService = restartService;
        this.logger = logger;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();
        this.repositoryChangeDebounceTimer = this.dispatcherQueue.CreateTimer();
        this.filterTextDebounceTimer = this.dispatcherQueue.CreateTimer();

        this.LoadDatabasesAsync(CancellationToken.None).FireAndForget((ex) => this.logger.LogError(ex, "Error loading databases."));

        this.databaseService.RepositoryChanged += this.DatabaseService_RepositoryChanged;
        this.disposables.Add(() => this.databaseService.RepositoryChanged -= this.DatabaseService_RepositoryChanged);
    }

    [ObservableProperty]
    public partial DatabaseTreeItemViewModel? Selected { get; set; }

    public ObservableCollection<DatabaseTreeItemViewModel> DataSource { get; } = [];

    [ObservableProperty]
    public partial ObservableCollection<DatabaseTreeItemBreadcrumbViewModel> Breadcrumbs { get; set; } = [];

    [ObservableProperty]
    public partial bool IsConfigured { get; set; } = false;

    [ObservableProperty]
    public partial bool IsDatabaseReloadPendingNotificationOpen { get; set; } = false;

    [ObservableProperty]
    public partial string FilterText { get; set; } = string.Empty;

    public static string EncodeNavigationParameter(DatabaseLibraryViewType view, DatabaseFile? databaseFile)
    {
        return string.Format("{0}|{1}", view, databaseFile?.FullPath ?? string.Empty);
    }

    public void Dispose()
    {
        this.disposables.TryDispose();
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is string encoded)
        {
            var parts = encoded.Split('|', 2);
            if (parts.Length == 2 && Enum.TryParse<DatabaseLibraryViewType>(parts[0], out var view))
            {
                var fullFileOrFolderPath = parts[1];
                var item = this.FindTreeItem(fullFileOrFolderPath);
                if (item is not null)
                {
                    // TBD: We don't have the necessary code in the view model to select the right view.
                    this.Selected = item;
                }
            }
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private DatabaseTreeItemViewModel? FindTreeItem(string fullFileOrFolderPath)
    {
        return FindItem(this.DataSource, fullFileOrFolderPath);

        static DatabaseTreeItemViewModel? FindItem(ObservableCollection<DatabaseTreeItemViewModel> items, string fullFileOrFolderPath)
        {
            foreach (var item in items)
            {
                if (item is DatabaseFolderTreeItemViewModel folderItem)
                {
                    if (string.Equals(folderItem.DatabaseFolder.FullPath, fullFileOrFolderPath, StringComparison.OrdinalIgnoreCase))
                    {
                        return folderItem;
                    }

                    var foundInChildren = FindItem(folderItem.Children, fullFileOrFolderPath);
                    if (foundInChildren is not null)
                    {
                        return foundInChildren;
                    }
                }
                else if (item is DatabaseFileTreeItemViewModel fileItem)
                {
                    if (string.Equals(fileItem.DatabaseFile.FullPath, fullFileOrFolderPath, StringComparison.OrdinalIgnoreCase))
                    {
                        return fileItem;
                    }
                }
            }

            return null;
        }
    }

    [RelayCommand]
    private async Task RestartAsync()
    {
        await this.restartService.RestartAsync();
    }

    [RelayCommand]
    private void OpenSettings()
    {
        try
        {
            this.navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    private async Task LoadDatabasesAsync(CancellationToken ct)
    {
        var repository = await this.databaseService.GetRepositoryAsync(ct);

        this.IsConfigured = repository.IsConfigured;

        foreach (var rootFolder in repository.RootFolders.OrderBy(f => f.Name))
        {
            var folderViewModel = await CreateFolderViewModelAsync(
                null,
                rootFolder,
                this.databaseService,
                this.clipboardService,
                this.filePickerService,
                this.fileExplorerService,
                this.operationCompletionService,
                this.dateTimeProviderService,
                this.dispatcherQueue,
                this.logger,
                ct);

            this.DataSource.Add(folderViewModel);
        }

        static async Task<DatabaseFolderTreeItemViewModel> CreateFolderViewModelAsync(
            DatabaseFolderTreeItemViewModel? parentFolderItemViewModel,
            DatabaseFolder folder,
            IDatabaseService databaseService,
            IClipboardService clipboardService,
            IFilePickerService filePickerService,
            IFileExplorerService fileExplorerService,
            IOperationCompletionService operationCompletionService,
            IDateTimeProviderService dateTimeProviderService,
            IDispatcherQueue dispatcherQueue,
            ILogger logger,
            CancellationToken ct)
        {
            var folderViewModel = await DatabaseFolderViewModel.CreateAsync(
                folder,
                databaseService,
                clipboardService,
                filePickerService,
                fileExplorerService,
                operationCompletionService,
                dateTimeProviderService,
                dispatcherQueue,
                logger,
                ct);

            var folderItemViewModel = new DatabaseFolderTreeItemViewModel(
                parentFolderItemViewModel,
                folder,
                folder.Name,
                Path.Combine(folder.RootAbsoluteFolderPath, folder.RelativePath),
                folderViewModel,
                clipboardService,
                fileExplorerService,
                logger);

            foreach (var subFolder in folder.SubFolders.OrderBy(f => f.Name))
            {
                var subFolderItemViewModel = await CreateFolderViewModelAsync(
                    folderItemViewModel,
                    subFolder,
                    databaseService,
                    clipboardService,
                    filePickerService,
                    fileExplorerService,
                    operationCompletionService,
                    dateTimeProviderService,
                    dispatcherQueue,
                    logger,
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
                    dispatcherQueue,
                    logger);

                folderItemViewModel.Children.Add(fileItemViewModel);
            }

            return folderItemViewModel;
        }
    }

    private void DatabaseService_RepositoryChanged(object? sender, EventArgs e)
    {
        this.repositoryChangeDebounceTimer.Debounce(DoReload, TimeSpan.FromMilliseconds(2000), immediate: false);

        void DoReload()
        {
            try
            {
                if (!IsAnyWorking(this.DataSource))
                {
                    this.DataSource.Clear();

                    this.LoadDatabasesAsync(CancellationToken.None).FireAndForget((ex) => this.logger.LogError(ex, "Error loading databases."));
                }
                else
                {
                    this.IsDatabaseReloadPendingNotificationOpen = true;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to reload database repository.");
            }
        }

        static bool IsAnyWorking(ObservableCollection<DatabaseTreeItemViewModel> items)
        {
            foreach (var item in items)
            {
                if (item is DatabaseFolderTreeItemViewModel folderItem)
                {
                    if (IsAnyWorking(folderItem.Children))
                    {
                        return true;
                    }
                }
                else if (item is DatabaseFileTreeItemViewModel fileItem)
                {
                    if (fileItem.Database?.IsScanning == true || fileItem.Database?.IsRebuilding == true)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    partial void OnFilterTextChanged(string value)
    {
        this.filterTextDebounceTimer.Debounce(() =>
        {
            foreach (var child in this.DataSource)
            {
                child.ApplyFilter(value);
            }
        }, TimeSpan.FromMilliseconds(500), immediate: string.IsNullOrWhiteSpace(value));
    }

    partial void OnSelectedChanged(DatabaseTreeItemViewModel? value)
    {
        // Load the database contents on demand when the user selects it the first time.
        if (value is DatabaseFileTreeItemViewModel item && item.Database is null)
        {
            this.ReloadDatabaseItemAsync(item, CancellationToken.None).FireAndForget((ex) => this.logger.LogError(ex, "Error reloading database."));
        }

        this.UpdateBreadcrumbs(value);
    }

    private void UpdateBreadcrumbs(DatabaseTreeItemViewModel? value)
    {
        // ObservableCollection was occasionally crashing with unspecified COM error
        // when modifying its contents, so recreate the collection instead.
        if (value is not null)
        {
            var list = new List<DatabaseTreeItemBreadcrumbViewModel>();

            DatabaseTreeItemViewModel? current = value;
            while (current is not null)
            {
                list.Add(new DatabaseTreeItemBreadcrumbViewModel(current));
                current = current.ParentFolderItem;
            }

            list.Reverse();

            this.Breadcrumbs = new ObservableCollection<DatabaseTreeItemBreadcrumbViewModel>(list);
        }
        else
        {
            this.Breadcrumbs = [];
        }
    }

    private async Task ReloadDatabaseItemAsync(DatabaseFileTreeItemViewModel item, CancellationToken ct)
    {
        var database = await this.databaseService.GetDatabaseAsync(item.DatabaseFile, ct);
        if (database is not null)
        {
            var parent = item.ParentFolderItem;
            if (parent is null)
            {
                throw new InvalidOperationException("ParentFolderItem cannot be null.");
            }

            var parentExpanded = parent.IsExpanded;

            var index = parent.Children.IndexOf(item);
            parent.Children.RemoveAt(index);

            var databaseViewModel = await DatabaseFileViewModel.CreateAsync(
                item.DatabaseFile,
                database,
                this.databaseService,
                this.clipboardService,
                this.filePickerService,
                this.fileExplorerService,
                this.operationCompletionService,
                this.dateTimeProviderService,
                this.dispatcherQueue,
                this.logger,
                ct);

            var databaseItemViewModel = item.WithDatabase(databaseViewModel);

            databaseItemViewModel.MatchedFilesCount = databaseViewModel.ScanMatchedFiles.Count;
            databaseItemViewModel.MissingFilesCount = databaseViewModel.ScanMissingFiles.Count;
            databaseItemViewModel.WrongNamedFilesCount = databaseViewModel.ScanIncorrectNameFiles.Count;
            databaseItemViewModel.UnusedFilesCount = databaseViewModel.ScanUnusedFiles.Count;

            parent.Children.Insert(index, databaseItemViewModel);
            parent.IsExpanded = parentExpanded;

            // Removing the old item above sets the Selected property to null.
            // Set it again unless user has already selected a different item.
            if (this.Selected is null || this.Selected == item)
            {
                this.Selected = databaseItemViewModel;
            }

            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
