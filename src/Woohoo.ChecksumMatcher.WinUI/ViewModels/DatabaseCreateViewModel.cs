// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Models;

public sealed partial class DatabaseCreateViewModel : ObservableObject, IDisposable
{
    private const string RedumpTimeFormat = "yyyy-MM-dd HH-mm-ss";
    private const string NoIntroTimeFormat = "yyyyMMdd-HHmmss";

    private readonly DisposableBag disposables = DisposableBag.Create<DatabaseCreateViewModel>();

    private readonly IDatabaseService databaseService;
    private readonly IFilePickerService filePickerService;
    private readonly IOperationCompletionService operationCompletionService;
    private readonly IDateTimeProviderService dateTimeProviderService;
    private readonly IDispatcherQueue dispatcherQueue;

    private CancellationTokenSource? cancellationTokenSource;

    public DatabaseCreateViewModel(
        IDatabaseService databaseService,
        IFilePickerService filePickerService,
        IOperationCompletionService operationCompletionService,
        IDateTimeProviderService dateTimeProviderService,
        IDispatcherQueueService dispatcherQueueService)
    {
        ArgumentNullException.ThrowIfNull(databaseService);
        ArgumentNullException.ThrowIfNull(filePickerService);
        ArgumentNullException.ThrowIfNull(operationCompletionService);
        ArgumentNullException.ThrowIfNull(dateTimeProviderService);
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);

        this.databaseService = databaseService;
        this.filePickerService = filePickerService;
        this.operationCompletionService = operationCompletionService;
        this.dateTimeProviderService = dateTimeProviderService;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();

        this.databaseService.DatabaseCreateProgress += this.DatabaseService_DatabaseCreateProgress;
        this.disposables.Add(() => this.databaseService.DatabaseCreateProgress -= this.DatabaseService_DatabaseCreateProgress);
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDatabaseCommand))]
    public partial string SourceFolder { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDatabaseCommand))]
    public partial string DatabaseFilePath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DatabaseName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DatabaseAuthor { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DatabaseVersion { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DatabaseUrl { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StatusViewModel Status { get; set; } = new StatusViewModel();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDatabaseCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyCanExecuteChangedFor(nameof(InsertDateNoIntroCommand))]
    [NotifyCanExecuteChangedFor(nameof(InsertDateRedumpCommand))]
    public partial bool IsScanning { get; set; }

    public void Dispose()
    {
        this.disposables.TryDispose();
    }

    [RelayCommand]
    private async Task BrowseSourceFolderAsync()
    {
        var folderPath = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.CreateDatabaseSourceFolder);
        if (!string.IsNullOrEmpty(folderPath))
        {
            this.SourceFolder = folderPath;
        }
    }

    [RelayCommand]
    private async Task BrowseDatabaseFileAsync()
    {
        var filePath = await this.filePickerService.GetSaveFilePathAsync(
            FilePickerSettingIdentifiers.CreateDatabaseTargetFile,
            (Localized.CreateDatabaseSaveFilter, ".dat"));

        if (!string.IsNullOrEmpty(filePath))
        {
            this.DatabaseFilePath = filePath;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreateDatabase))]
    private async Task CreateDatabaseAsync()
    {
        if (!Directory.Exists(this.SourceFolder))
        {
            return;
        }

        this.cancellationTokenSource = new CancellationTokenSource();

        this.IsScanning = true;

        try
        {
            var settings = new DatabaseCreateSettings
            {
                Author = this.DatabaseAuthor,
                Name = this.DatabaseName,
                Description = this.DatabaseName,
                Version = this.DatabaseVersion,
                Url = this.DatabaseUrl,
                ForceCalculateChecksums = true,
            };

            await this.databaseService.CreateDatabaseAsync(
                this.SourceFolder,
                this.DatabaseFilePath,
                settings,
                this.cancellationTokenSource.Token);

            this.Status = new StatusViewModel(string.Format(CultureInfo.CurrentUICulture, Localized.DatabaseCreateProgressCompleteFormat, this.dateTimeProviderService.Now), StatusSeverity.Success);

            await this.operationCompletionService.NotifyCompletionWithOpenInExplorer(
                OperationCompletionResult.Success,
                Localized.CreateDatabaseSuccessNotification,
                this.DatabaseFilePath,
                CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            this.Status = new StatusViewModel(Localized.DatabaseCreateProgressCanceled, StatusSeverity.Error);

            await this.operationCompletionService.NotifyCompletion(
                OperationCompletionResult.Cancelled,
                Localized.CreateDatabaseCancelNotification,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            this.Status = new StatusViewModel(string.Format(CultureInfo.CurrentUICulture, Localized.DatabaseCreateProgressErrorFormat, ex.Message), StatusSeverity.Error);

            await this.operationCompletionService.NotifyCompletion(
                OperationCompletionResult.Error,
                Localized.CreateDatabaseErrorNotification,
                CancellationToken.None);
        }
        finally
        {
            this.IsScanning = false;
            this.cancellationTokenSource?.Dispose();
            this.cancellationTokenSource = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        this.cancellationTokenSource?.Cancel();
        this.CancelCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanInsertDate))]
    private void InsertDateRedump()
    {
        this.DatabaseVersion = this.dateTimeProviderService.UtcNow.ToString(RedumpTimeFormat);
    }

    [RelayCommand(CanExecute = nameof(CanInsertDate))]
    private void InsertDateNoIntro()
    {
        this.DatabaseVersion = this.dateTimeProviderService.UtcNow.ToString(NoIntroTimeFormat);
    }

    private bool CanInsertDate()
    {
        return !this.IsScanning;
    }

    private bool CanCreateDatabase()
    {
        return !string.IsNullOrEmpty(this.SourceFolder) && !string.IsNullOrEmpty(this.DatabaseFilePath) && !this.IsScanning;
    }

    private bool CanCancel()
    {
        return this.IsScanning &&
            this.cancellationTokenSource is not null &&
            !this.cancellationTokenSource.IsCancellationRequested;
    }

    private void ResetStatus()
    {
        if (File.Exists(this.DatabaseFilePath))
        {
            this.Status = new StatusViewModel(Localized.CreateDatabaseStatusFileExists, StatusSeverity.Error);
        }
        else
        {
            this.Status = new StatusViewModel();
        }
    }

    private void DatabaseService_DatabaseCreateProgress(object? sender, DatabaseCreateEventArgs e)
    {
        this.dispatcherQueue.TryEnqueue(() =>
        {
            switch (e.Status)
            {
                case DatabaseCreateStatus.Pending:
                    this.Status = new StatusViewModel(Localized.DatabaseCreateProgressPending, StatusSeverity.Info);
                    break;
                case DatabaseCreateStatus.Started:
                    this.Status = new StatusViewModel(Localized.DatabaseCreateProgressStarted, StatusSeverity.Info);
                    break;
                case DatabaseCreateStatus.Scanning:
                    this.Status = new StatusViewModel(Localized.DatabaseCreateProgressScanning, StatusSeverity.Info);
                    break;
                case DatabaseCreateStatus.Hashing:
                    Debug.Assert(e.HashingFile is not null, "Hashing file should not be null in Hashing status.");
                    this.Status = new StatusViewModel(
                        string.Format(
                            CultureInfo.CurrentUICulture,
                            Localized.DatabaseCreateProgressHashingFormat,
                            $"{e.HashingFile?.ContainerName}\\{e.HashingFile?.RomRelativeFilePath}"),
                        StatusSeverity.Info);
                    break;
                default:
                    break;
            }
        });
    }

    partial void OnDatabaseFilePathChanged(string value)
    {
        this.ResetStatus();
    }

    partial void OnDatabaseAuthorChanged(string value)
    {
        this.ResetStatus();
    }

    partial void OnDatabaseNameChanged(string value)
    {
        this.ResetStatus();
    }

    partial void OnDatabaseUrlChanged(string value)
    {
        this.ResetStatus();
    }

    partial void OnDatabaseVersionChanged(string value)
    {
        this.ResetStatus();
    }
}
