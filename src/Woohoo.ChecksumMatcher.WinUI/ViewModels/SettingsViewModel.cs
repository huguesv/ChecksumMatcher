// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Models;

public sealed partial class SettingsViewModel : ObservableRecipient
{
    private readonly IAboutInformationService aboutInformationService;
    private readonly IThemeSelectorService themeSelectorService;
    private readonly ILocalSettingsService localSettingsService;
    private readonly IConfirmationService confirmationService;
    private readonly IFilePickerService filePickerService;
    private readonly IFileExplorerService fileExplorerService;
    private readonly IDatabaseService databaseService;
    private readonly IOfflineExplorerService offlineExplorerService;
    private readonly IRedumpWebService redumpWebService;
    private readonly IOperationCompletionService operationCompletionService;
    private readonly IDateTimeProviderService dateTimeProviderService;
    private readonly ILogger logger;
    private readonly IDispatcherQueue dispatcherQueue;

    private bool isLoadingSettings = false;
    private CancellationTokenSource? redumpDownloadCancellationTokenSource;

    public SettingsViewModel(
        IAboutInformationService aboutInformationService,
        IThemeSelectorService themeSelectorService,
        ILocalSettingsService localSettingsService,
        IConfirmationService confirmationService,
        IFilePickerService filePickerService,
        IFileExplorerService fileExplorerService,
        IDatabaseService databaseService,
        IOfflineExplorerService offlineExplorerService,
        IRedumpWebService redumpWebService,
        IOperationCompletionService operationCompletionService,
        IDateTimeProviderService dateTimeProviderService,
        IDispatcherQueueService dispatcherQueueService,
        ILogger<SettingsViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(aboutInformationService);
        ArgumentNullException.ThrowIfNull(themeSelectorService);
        ArgumentNullException.ThrowIfNull(localSettingsService);
        ArgumentNullException.ThrowIfNull(confirmationService);
        ArgumentNullException.ThrowIfNull(filePickerService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);
        ArgumentNullException.ThrowIfNull(databaseService);
        ArgumentNullException.ThrowIfNull(offlineExplorerService);
        ArgumentNullException.ThrowIfNull(redumpWebService);
        ArgumentNullException.ThrowIfNull(operationCompletionService);
        ArgumentNullException.ThrowIfNull(dateTimeProviderService);
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);
        ArgumentNullException.ThrowIfNull(logger);

        this.aboutInformationService = aboutInformationService;
        this.themeSelectorService = themeSelectorService;
        this.localSettingsService = localSettingsService;
        this.confirmationService = confirmationService;
        this.filePickerService = filePickerService;
        this.fileExplorerService = fileExplorerService;
        this.databaseService = databaseService;
        this.offlineExplorerService = offlineExplorerService;
        this.redumpWebService = redumpWebService;
        this.operationCompletionService = operationCompletionService;
        this.dateTimeProviderService = dateTimeProviderService;
        this.logger = logger;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();

        this.ElementTheme = this.themeSelectorService.Theme;

        this.AboutInfo = this.aboutInformationService.GetInformation();

        this.isLoadingSettings = true;
        try
        {
            this.UseWindowsNotifications = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.UseWindowsNotifications) ?? true;
            this.UseSystemSounds = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.UseSystemSounds) ?? false;

            this.ExplorerShowArchivesInTree = this.localSettingsService.ReadSetting<bool>(KnownSettingKeys.ExplorerShowArchiveInTree);
            this.ExplorerFolderSortPriority = Enum.Parse<OfflineExplorerFolderSortPriority>(this.localSettingsService.ReadSetting<string>(KnownSettingKeys.ExplorerFolderSortPriority) ?? OfflineExplorerFolderSortPriority.First.ToString());
            this.ExplorerArchiveGrouping = Enum.Parse<OfflineExplorerArchiveGrouping>(this.localSettingsService.ReadSetting<string>(KnownSettingKeys.ExplorerArchiveGrouping) ?? OfflineExplorerArchiveGrouping.WithFiles.ToString());

            this.RedumpDownloadFolder = this.CreateFolderViewModel(this.localSettingsService.ReadSetting<string>(KnownSettingKeys.RedumpDownloadFolder) ?? string.Empty, this.RemoveRedumpFolderCommand);
            this.RedumpUser = this.localSettingsService.LoadEncryptedSetting(KnownSettingKeys.RedumpUser, string.Empty);
            this.RedumpPassword = this.localSettingsService.LoadEncryptedSetting(KnownSettingKeys.RedumpPassword, string.Empty);

            var redumpSystems = this.redumpWebService.GetSystems().OrderBy(s => s.Name);
            var redumpEnabledSystems = this.localSettingsService.ReadSetting<string[]>(KnownSettingKeys.RedumpSystems);
            if (redumpEnabledSystems is null)
            {
                this.RedumpSystems = [.. redumpSystems.Select(s => new SettingsRedumpSystemViewModel(s.Id, s.Name, true, this.OnRedumpSystemChanged))];
            }
            else
            {
                this.RedumpSystems = [.. redumpSystems.Select(s => new SettingsRedumpSystemViewModel(s.Id, s.Name, redumpEnabledSystems.Contains(s.Id), this.OnRedumpSystemChanged))];
            }
        }
        finally
        {
            this.isLoadingSettings = false;
        }

        this.LoadSettingsAsync(CancellationToken.None).FireAndForget((ex) => this.logger.LogError(ex, "Error loading settings."));
    }

    public AboutInformation AboutInfo { get; }

    [ObservableProperty]
    public partial ElementTheme ElementTheme { get; set; }

    [ObservableProperty]
    public partial bool UseWindowsNotifications { get; set; }

    [ObservableProperty]
    public partial bool UseSystemSounds { get; set; }

    [ObservableProperty]
    public partial bool ExplorerShowArchivesInTree { get; set; }

    [ObservableProperty]
    public partial OfflineExplorerFolderSortPriority ExplorerFolderSortPriority { get; set; }

    [ObservableProperty]
    public partial OfflineExplorerArchiveGrouping ExplorerArchiveGrouping { get; set; }

    public ObservableCollection<SettingsFolderViewModel> DatabaseFolders { get; private set; } = [];

    public ObservableCollection<SettingsFolderViewModel> CueFolders { get; private set; } = [];

    public ObservableCollection<SettingsFolderViewModel> OfflineStorageFolders { get; private set; } = [];

    public ObservableCollection<SettingsRedumpSystemViewModel> RedumpSystems { get; private set; } = [];

    [ObservableProperty]
    public partial string RedumpUser { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RedumpPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StatusViewModel RedumpCredentialsStatus { get; set; } = new(string.Empty, StatusSeverity.None);

    [ObservableProperty]
    public partial StatusViewModel RedumpDownloadStatus { get; set; } = new(string.Empty, StatusSeverity.None);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadRedumpArtifactsCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveRedumpFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteRedumpDownloadFolderContentsCommand))]
    public partial SettingsFolderViewModel RedumpDownloadFolder { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadRedumpArtifactsCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelDownloadRedumpArtifactsCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteRedumpDownloadFolderContentsCommand))]
    public partial bool IsDownloading { get; set; } = false;

    [RelayCommand(CanExecute = nameof(CanRemoveRedumpFolder))]
    private Task RemoveRedumpFolderAsync()
    {
        try
        {
            this.RedumpDownloadFolder = this.CreateFolderViewModel();
            this.localSettingsService.SaveSetting(KnownSettingKeys.RedumpDownloadFolder, this.RedumpDownloadFolder.Path);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }

        return Task.CompletedTask;
    }

    private bool CanRemoveRedumpFolder()
    {
        return !string.IsNullOrEmpty(this.RedumpDownloadFolder.Path);
    }

    [RelayCommand(CanExecute = nameof(CanOpenFolder))]
    private void OpenFolder(string folder)
    {
        try
        {
            this.fileExplorerService.OpenInExplorer(folder);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    private bool CanOpenFolder(string folder)
    {
        return Directory.Exists(folder);
    }

    [RelayCommand]
    private void SelectAllRedumpSystems()
    {
        try
        {
            this.ChangeAllRedumpSystems(true);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private void UnselectAllRedumpSystems()
    {
        try
        {
            this.ChangeAllRedumpSystems(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    private void ChangeAllRedumpSystems(bool isEnabled)
    {
        this.isLoadingSettings = true;

        try
        {
            foreach (var system in this.RedumpSystems)
            {
                system.IsEnabled = isEnabled;
            }
        }
        finally
        {
            this.isLoadingSettings = false;
        }

        this.OnRedumpSystemChanged();
    }

    [RelayCommand]
    private async Task SelectRedumpFolderAsync(CancellationToken ct)
    {
        try
        {
            var folder = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.RedumpDownloadFolder);
            if (folder != null)
            {
                this.RedumpDownloadFolder = this.CreateFolderViewModel(folder, this.RemoveRedumpFolderCommand);
                this.localSettingsService.SaveSetting(KnownSettingKeys.RedumpDownloadFolder, this.RedumpDownloadFolder.Path);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private async Task TestRedumpCredentialsAsync(CancellationToken ct)
    {
        try
        {
            this.RedumpCredentialsStatus = new(Localized.RedumpCredentialsStatusInProgress, StatusSeverity.Info);

            var success = await ValidateCredentialsAsync(this.redumpWebService, this.RedumpUser, this.RedumpPassword, ct);

            if (success)
            {
                this.RedumpCredentialsStatus = new(Localized.RedumpCredentialsStatusSuccess, StatusSeverity.Success);
            }
            else
            {
                this.RedumpCredentialsStatus = new(Localized.RedumpCredentialsStatusError, StatusSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }

        static async Task<bool> ValidateCredentialsAsync(IRedumpWebService redumpDownloaderService, string user, string password, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            return (await redumpDownloaderService.ValidateCredentialsAsync(user, password, ct)) == true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelDownloadRedumpArtifacts))]
    private void CancelDownloadRedumpArtifacts()
    {
        try
        {
            this.redumpDownloadCancellationTokenSource?.Cancel();
            this.CancelDownloadRedumpArtifactsCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    private bool CanCancelDownloadRedumpArtifacts()
    {
        return this.IsDownloading &&
            this.redumpDownloadCancellationTokenSource is not null &&
            !this.redumpDownloadCancellationTokenSource.IsCancellationRequested;
    }

    [RelayCommand(CanExecute = nameof(CanDownloadRedumpArtifacts))]
    private async Task DownloadRedumpArtifacts()
    {
        try
        {
            this.redumpDownloadCancellationTokenSource = new CancellationTokenSource();

            this.IsDownloading = true;

            try
            {
                this.RedumpDownloadStatus = new StatusViewModel(Localized.RedumpDownloadStatusStarted, StatusSeverity.Info);

                var ids = this.RedumpSystems.Where(s => s.IsEnabled).Select(s => s.Id).ToArray();

                await this.redumpWebService.DownloadAsync(ids, this.RedumpDownloadFolder.Path, false, this.RedumpUser, this.RedumpPassword, ProgressUpdate, this.redumpDownloadCancellationTokenSource.Token);

                this.RedumpDownloadStatus = new StatusViewModel(string.Format(CultureInfo.CurrentUICulture, Localized.RedumpDownloadStatusCompleteFormat, this.dateTimeProviderService.Now), StatusSeverity.Success);

                await this.operationCompletionService.NotifyCompletion(
                    OperationCompletionResult.Success,
                    Localized.DownloadSuccessNotification,
                    CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                this.RedumpDownloadStatus = new StatusViewModel(Localized.RedumpDownloadStatusCanceled, StatusSeverity.Error);
                await this.operationCompletionService.NotifyCompletion(
                    OperationCompletionResult.Cancelled,
                    Localized.DownloadCancelNotification,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                this.RedumpDownloadStatus = new StatusViewModel(string.Format(CultureInfo.CurrentUICulture, Localized.RedumpDownloadStatusErrorFormat, ex.Message), StatusSeverity.Error);
                await this.operationCompletionService.NotifyCompletion(
                    OperationCompletionResult.Error,
                    Localized.DownloadErrorNotification,
                    CancellationToken.None);
            }
            finally
            {
                this.IsDownloading = false;
                this.redumpDownloadCancellationTokenSource?.Dispose();
                this.redumpDownloadCancellationTokenSource = null;
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }

        void ProgressUpdate(int current, int max, string systemName, string artifactName)
        {
            if (string.IsNullOrEmpty(systemName) || string.IsNullOrEmpty(artifactName))
            {
                return;
            }

            this.dispatcherQueue.TryEnqueue(() =>
            {
                this.RedumpDownloadStatus = new StatusViewModel(string.Format(CultureInfo.CurrentUICulture, Localized.RedumpDownloadStatusInProgressFormat, current + 1, max, artifactName, systemName), StatusSeverity.Info);
            });
        }
    }

    private bool CanDownloadRedumpArtifacts()
    {
        return Directory.Exists(this.RedumpDownloadFolder.Path) && !this.IsDownloading;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteRedumpDownloadFolderContents))]
    private async Task DeleteRedumpDownloadFolderContentsAsync()
    {
        try
        {
            var result = await this.confirmationService.ShowAsync(
                Localized.MainWindowCaption,
                string.Format(CultureInfo.CurrentUICulture, Localized.SettingsPageDeleteRedumpFolderContentsMessageFormat, this.RedumpDownloadFolder.Path),
                defaultToCancel: true);

            if (!result)
            {
                return;
            }

            foreach (var file in Directory.EnumerateFiles(this.RedumpDownloadFolder.Path))
            {
                FileUtility.SafeDelete(file);
            }

            foreach (var folder in Directory.EnumerateDirectories(this.RedumpDownloadFolder.Path))
            {
                FileUtility.SafeDeleteFolder(folder, recursive: true);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    private bool CanDeleteRedumpDownloadFolderContents()
    {
        return Directory.Exists(this.RedumpDownloadFolder.Path) && !this.IsDownloading;
    }

    [RelayCommand]
    private async Task AddDatabaseFolderAsync(CancellationToken ct)
    {
        try
        {
            var folder = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.DatabaseFolder);
            if (folder != null)
            {
                if (this.DatabaseFolders.Any(f => f.Path == folder))
                {
                    return;
                }

                this.DatabaseFolders.Add(this.CreateFolderViewModel(folder, this.RemoveDatabaseFolderCommand));

                await this.databaseService.AddRepositoryFolderAsync(folder, ct);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private async Task RemoveDatabaseFolderAsync(string folder, CancellationToken ct)
    {
        try
        {
            if (folder is not null)
            {
                var viewModel = this.DatabaseFolders.FirstOrDefault(f => f.Path == folder);
                if (viewModel is not null)
                {
                    this.DatabaseFolders.Remove(viewModel);

                    await this.databaseService.RemoveRepositoryFolderAsync(folder, ct);
                }
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private async Task AddCueFolderAsync(CancellationToken ct)
    {
        try
        {
            var folder = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.CueFolder);
            if (folder != null)
            {
                if (this.CueFolders.Any(f => f.Path == folder))
                {
                    return;
                }

                this.CueFolders.Add(this.CreateFolderViewModel(folder, this.RemoveCueFolderCommand));

                await this.databaseService.AddCueFolderAsync(folder, ct);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private async Task RemoveCueFolderAsync(string folder, CancellationToken ct)
    {
        try
        {
            if (folder is not null)
            {
                var viewModel = this.CueFolders.FirstOrDefault(f => f.Path == folder);
                if (viewModel is not null)
                {
                    this.CueFolders.Remove(viewModel);

                    await this.databaseService.RemoveCueFolderAsync(folder, ct);
                }
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private async Task AddOfflineStorageFolderAsync(CancellationToken ct)
    {
        try
        {
            var folder = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.OfflineStorageFolder);
            if (folder != null)
            {
                if (this.OfflineStorageFolders.Any(f => f.Path == folder))
                {
                    return;
                }

                this.OfflineStorageFolders.Add(this.CreateFolderViewModel(folder, this.RemoveOfflineStorageFolderCommand));

                await this.offlineExplorerService.AddFolderAsync(folder, ct);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private async Task RemoveOfflineStorageFolderAsync(string folder, CancellationToken ct)
    {
        try
        {
            if (folder is not null)
            {
                var viewModel = this.OfflineStorageFolders.FirstOrDefault(f => f.Path == folder);
                if (viewModel is not null)
                {
                    this.OfflineStorageFolders.Remove(viewModel);

                    await this.offlineExplorerService.RemoveFolderAsync(folder, ct);
                }
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private async Task SwitchThemeAsync(ElementTheme theme)
    {
        try
        {
            if (this.ElementTheme != theme)
            {
                this.ElementTheme = theme;
                await this.themeSelectorService.SetThemeAsync(theme);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private void SwitchExplorerFolderSortPriority(int priority)
    {
        try
        {
            this.SwitchExplorerFolderSortPriority((OfflineExplorerFolderSortPriority)priority);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    private void SwitchExplorerFolderSortPriority(OfflineExplorerFolderSortPriority priority)
    {
        if (this.ExplorerFolderSortPriority != priority)
        {
            this.ExplorerFolderSortPriority = priority;
            this.localSettingsService.SaveSetting(KnownSettingKeys.ExplorerFolderSortPriority, priority.ToString());
        }
    }

    [RelayCommand]
    private void SwitchExplorerArchiveGrouping(int grouping)
    {
        try
        {
            this.SwitchExplorerArchiveGrouping((OfflineExplorerArchiveGrouping)grouping);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private void OpenSettingsFileInExplorer()
    {
        try
        {
            var settingsFile = this.localSettingsService.FilePath;
            if (File.Exists(settingsFile))
            {
                this.fileExplorerService.OpenInExplorer(settingsFile);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    private async Task LoadSettingsAsync(CancellationToken ct)
    {
        var repositoryFolders = await this.databaseService.GetRepositoryFoldersAsync(ct);
        var cueFolders = await this.databaseService.GetCueFoldersAsync(ct);
        var offlineFolders = await this.offlineExplorerService.GetFoldersAsync(ct);

        this.DatabaseFolders = [.. repositoryFolders.Where(f => !string.IsNullOrEmpty(f)).Select(f => this.CreateFolderViewModel(f, this.RemoveDatabaseFolderCommand))];
        this.CueFolders = [.. cueFolders.Where(f => !string.IsNullOrEmpty(f)).Select(f => this.CreateFolderViewModel(f, this.RemoveCueFolderCommand))];
        this.OfflineStorageFolders = [.. offlineFolders.Where(f => !string.IsNullOrEmpty(f)).Select(f => this.CreateFolderViewModel(f, this.RemoveOfflineStorageFolderCommand))];
    }

    private void SwitchExplorerArchiveGrouping(OfflineExplorerArchiveGrouping grouping)
    {
        if (this.ExplorerArchiveGrouping != grouping)
        {
            this.ExplorerArchiveGrouping = grouping;
            this.localSettingsService.SaveSetting(KnownSettingKeys.ExplorerArchiveGrouping, grouping.ToString());
        }
    }

    private void OnRedumpSystemChanged()
    {
        if (!this.isLoadingSettings)
        {
            this.localSettingsService.SaveSetting<string[]>(
                KnownSettingKeys.RedumpSystems,
                [.. this.RedumpSystems.Where(s => s.IsEnabled).Select(s => s.Id)]);
        }
    }

    private SettingsFolderViewModel CreateFolderViewModel(string path, IAsyncRelayCommand removeCommand)
    {
        return new SettingsFolderViewModel(this.fileExplorerService, this.dispatcherQueue, this.logger, path, removeCommand);
    }

    private SettingsFolderViewModel CreateFolderViewModel()
    {
        return new SettingsFolderViewModel(this.fileExplorerService, this.dispatcherQueue, this.logger);
    }

    partial void OnExplorerShowArchivesInTreeChanged(bool value)
    {
        this.localSettingsService.SaveSetting(KnownSettingKeys.ExplorerShowArchiveInTree, value);
    }

    partial void OnRedumpUserChanged(string value)
    {
        if (!this.isLoadingSettings)
        {
            this.RedumpCredentialsStatus = new(string.Empty, StatusSeverity.None);

            this.localSettingsService.SaveEncryptedSetting(KnownSettingKeys.RedumpUser, value);
        }
    }

    partial void OnRedumpPasswordChanged(string value)
    {
        if (!this.isLoadingSettings)
        {
            this.RedumpCredentialsStatus = new(string.Empty, StatusSeverity.None);

            this.localSettingsService.SaveEncryptedSetting(KnownSettingKeys.RedumpPassword, value);
        }
    }

    partial void OnUseSystemSoundsChanged(bool value)
    {
        if (!this.isLoadingSettings)
        {
            this.localSettingsService.SaveSetting(KnownSettingKeys.UseSystemSounds, value);
        }
    }

    partial void OnUseWindowsNotificationsChanged(bool value)
    {
        if (!this.isLoadingSettings)
        {
            this.localSettingsService.SaveSetting(KnownSettingKeys.UseWindowsNotifications, value);
        }
    }
}
