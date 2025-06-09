// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        INavigationService navigationService)
    {
        ArgumentNullException.ThrowIfNull(databaseService);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(filePickerService);
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);
        ArgumentNullException.ThrowIfNull(operationCompletionService);
        ArgumentNullException.ThrowIfNull(dateTimeProviderService);
        ArgumentNullException.ThrowIfNull(navigationService);

        this.databaseService = databaseService;
        this.clipboardService = clipboardService;
        this.filePickerService = filePickerService;
        this.fileExplorerService = fileExplorerService;
        this.operationCompletionService = operationCompletionService;
        this.dateTimeProviderService = dateTimeProviderService;
        this.navigationService = navigationService;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();
        this.repositoryChangeDebounceTimer = this.dispatcherQueue.CreateTimer();
        this.filterTextDebounceTimer = this.dispatcherQueue.CreateTimer();

        this.LoadDatabasesAsync(CancellationToken.None).FireAndForget();

        this.databaseService.RepositoryChanged += this.DatabaseService_RepositoryChanged;
        this.disposables.Add(() => this.databaseService.RepositoryChanged -= this.DatabaseService_RepositoryChanged);
    }

    [ObservableProperty]
    public partial DatabaseTreeItemViewModel? Selected { get; set; }

    public ObservableCollection<DatabaseTreeItemViewModel> DataSource { get; } = [];

    public ObservableCollection<DatabaseTreeItemBreadcrumbViewModel> Breadcrumbs { get; } = [];

    [ObservableProperty]
    public partial bool IsConfigured { get; set; } = false;

    [ObservableProperty]
    public partial string FilterText { get; set; } = string.Empty;

    public void Dispose()
    {
        this.disposables.TryDispose();
    }

    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    private void OpenSettings()
    {
        this.navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
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
        this.repositoryChangeDebounceTimer.Debounce(DoReload, TimeSpan.FromMilliseconds(2000), immediate: false);

        void DoReload()
        {
            bool anyWorking = this.DataSource.Any(item => item is DatabaseFolderTreeItemViewModel folder && folder.Children.Any(child => child is DatabaseFileTreeItemViewModel file && (file.Database?.IsScanning == true || file.Database?.IsRebuilding == true)));
            if (!anyWorking)
            {
                this.DataSource.Clear();

                this.LoadDatabasesAsync(CancellationToken.None).FireAndForget();
            }
            else
            {
                // TODO: Need to postpone until the work is completed, or notify user to restart later
            }
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
            this.ReloadDatabaseItemAsync(item, CancellationToken.None).FireAndForget();
        }

        this.Breadcrumbs.Clear();
        if (value is not null)
        {
            List<DatabaseTreeItemBreadcrumbViewModel> list = new List<DatabaseTreeItemBreadcrumbViewModel>();

            DatabaseTreeItemViewModel? current = value;
            while (current is not null)
            {
                list.Add(new DatabaseTreeItemBreadcrumbViewModel(current));
                current = current.ParentFolderItem;
            }

            list.Reverse();

            foreach (var breadcrumb in list)
            {
                this.Breadcrumbs.Add(breadcrumb);
            }
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

            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
