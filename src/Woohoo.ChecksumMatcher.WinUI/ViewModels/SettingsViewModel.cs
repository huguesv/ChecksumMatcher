// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Models;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService themeSelectorService;
    private readonly ILocalSettingsService localSettingsService;
    private readonly IFilePickerService filePickerService;
    private readonly IFileExplorerService fileExplorerService;
    private readonly IDatabaseService databaseService;
    private readonly IOfflineExplorerService offlineExplorerService;
    private readonly IRedumpWebService redumpWebService;
    private readonly IOperationCompletionService operationCompletionService;
    private readonly IDateTimeProviderService dateTimeProviderService;
    private readonly IDispatcherQueue dispatcherQueue;

    private readonly bool isLoadingSettings = false;
    private CancellationTokenSource? redumpDownloadCancellationTokenSource;

    public SettingsViewModel(
        IThemeSelectorService themeSelectorService,
        ILocalSettingsService localSettingsService,
        IFilePickerService filePickerService,
        IFileExplorerService fileExplorerService,
        IDatabaseService databaseService,
        IOfflineExplorerService offlineExplorerService,
        IRedumpWebService redumpWebService,
        IOperationCompletionService operationCompletionService,
        IDateTimeProviderService dateTimeProviderService,
        IDispatcherQueueService dispatcherQueueService)
    {
        ArgumentNullException.ThrowIfNull(themeSelectorService);
        ArgumentNullException.ThrowIfNull(localSettingsService);
        ArgumentNullException.ThrowIfNull(filePickerService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);
        ArgumentNullException.ThrowIfNull(databaseService);
        ArgumentNullException.ThrowIfNull(offlineExplorerService);
        ArgumentNullException.ThrowIfNull(redumpWebService);
        ArgumentNullException.ThrowIfNull(operationCompletionService);
        ArgumentNullException.ThrowIfNull(dateTimeProviderService);
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);

        this.themeSelectorService = themeSelectorService;
        this.localSettingsService = localSettingsService;
        this.filePickerService = filePickerService;
        this.fileExplorerService = fileExplorerService;
        this.databaseService = databaseService;
        this.offlineExplorerService = offlineExplorerService;
        this.redumpWebService = redumpWebService;
        this.operationCompletionService = operationCompletionService;
        this.dateTimeProviderService = dateTimeProviderService;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();

        this.ElementTheme = this.themeSelectorService.Theme;
        this.VersionDescription = GetVersionDescription();
        this.Version = GetVersion();

        this.AboutFramework = RuntimeInformation.FrameworkDescription;
        this.AboutProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString();
        this.AboutRuntimeIdentifier = RuntimeInformation.RuntimeIdentifier;
        this.AboutOperatingSystem = string.Format(CultureInfo.CurrentUICulture, "{0} {1}", RuntimeInformation.OSDescription, RuntimeInformation.OSArchitecture);

        this.isLoadingSettings = true;
        try
        {
            this.UseWindowsNotifications = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.UseWindowsNotifications) ?? true;
            this.UseSystemSounds = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.UseSystemSounds) ?? false;

            this.ExplorerShowArchivesInTree = this.localSettingsService.ReadSetting<bool>(KnownSettingKeys.ExplorerShowArchiveInTree);
            this.ExplorerFolderSortPriority = Enum.Parse<OfflineExplorerFolderSortPriority>(this.localSettingsService.ReadSetting<string>(KnownSettingKeys.ExplorerFolderSortPriority) ?? OfflineExplorerFolderSortPriority.First.ToString());
            this.ExplorerArchiveGrouping = Enum.Parse<OfflineExplorerArchiveGrouping>(this.localSettingsService.ReadSetting<string>(KnownSettingKeys.ExplorerArchiveGrouping) ?? OfflineExplorerArchiveGrouping.WithFiles.ToString());

            this.RedumpDownloadFolder = this.localSettingsService.ReadSetting<string>(KnownSettingKeys.RedumpDownloadFolder) ?? string.Empty;
            this.RedumpUser = this.localSettingsService.ReadSetting<string>(KnownSettingKeys.RedumpUser) ?? string.Empty;
            this.RedumpPassword = this.localSettingsService.LoadEncryptedSetting(KnownSettingKeys.RedumpPassword, string.Empty);
        }
        finally
        {
            this.isLoadingSettings = false;
        }

        this.LoadSettingsAsync(CancellationToken.None).SafeFireAndForget();
    }

    [ObservableProperty]
    public partial ElementTheme ElementTheme { get; set; }

    [ObservableProperty]
    public partial string VersionDescription { get; set; }

    [ObservableProperty]
    public partial string Version { get; set; }

    [ObservableProperty]
    public partial string AboutFramework { get; set; }

    [ObservableProperty]
    public partial string AboutProcessArchitecture { get; set; }

    [ObservableProperty]
    public partial string AboutRuntimeIdentifier { get; set; }

    [ObservableProperty]
    public partial string AboutOperatingSystem { get; set; }

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

    public ObservableCollection<string> DatabaseFolders { get; private set; } = [];

    public ObservableCollection<string> CueFolders { get; private set; } = [];

    public ObservableCollection<string> OfflineStorageFolders { get; private set; } = [];

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
    [NotifyCanExecuteChangedFor(nameof(OpenRedumpFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveRedumpFolderCommand))]
    public partial string RedumpDownloadFolder { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadRedumpArtifactsCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelDownloadRedumpArtifactsCommand))]
    public partial bool IsDownloading { get; set; } = false;

    private static string GetVersion()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return string.Format(CultureInfo.CurrentUICulture, Localized.SettingsPageVersionFormat, version);
    }

    private static string GetVersionDescription()
    {
        return $"{"AppDisplayName".GetLocalized()} - {GetVersion()}";
    }

    [RelayCommand(CanExecute = nameof(CanRemoveRedumpFolder))]
    private void RemoveRedumpFolder()
    {
        this.RedumpDownloadFolder = string.Empty;
        this.localSettingsService.SaveSetting(KnownSettingKeys.RedumpDownloadFolder, this.RedumpDownloadFolder);
    }

    private bool CanRemoveRedumpFolder()
    {
        return !string.IsNullOrEmpty(this.RedumpDownloadFolder);
    }

    [RelayCommand(CanExecute = nameof(CanOpenRedumpFolder))]
    private void OpenRedumpFolder()
    {
        if (Directory.Exists(this.RedumpDownloadFolder))
        {
            this.fileExplorerService.OpenInExplorer(this.RedumpDownloadFolder);
        }
    }

    private bool CanOpenRedumpFolder()
    {
        return Directory.Exists(this.RedumpDownloadFolder);
    }

    [RelayCommand]
    private async Task SelectRedumpFolderAsync(CancellationToken ct)
    {
        var folder = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.RedumpDownloadFolder);
        if (folder != null)
        {
            this.RedumpDownloadFolder = folder;
            this.localSettingsService.SaveSetting(KnownSettingKeys.RedumpDownloadFolder, this.RedumpDownloadFolder);
        }
    }

    [RelayCommand]
    private async Task TestRedumpCredentialsAsync(CancellationToken ct)
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
        this.redumpDownloadCancellationTokenSource?.Cancel();
        this.CancelDownloadRedumpArtifactsCommand.NotifyCanExecuteChanged();
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
        this.redumpDownloadCancellationTokenSource = new CancellationTokenSource();

        this.IsDownloading = true;

        try
        {
            this.RedumpDownloadStatus = new StatusViewModel(Localized.RedumpDownloadStatusStarted, StatusSeverity.Info);

            await this.redumpWebService.DownloadAllAsync(this.RedumpDownloadFolder, false, this.RedumpUser, this.RedumpPassword, ProgressUpdate, this.redumpDownloadCancellationTokenSource.Token);

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
        return Directory.Exists(this.RedumpDownloadFolder) && !this.IsDownloading;
    }

    [RelayCommand]
    private async Task AddDatabaseFolderAsync(CancellationToken ct)
    {
        var folder = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.DatabaseFolder);
        if (folder != null)
        {
            if (this.DatabaseFolders.Contains(folder))
            {
                return;
            }

            this.DatabaseFolders.Add(folder);

            await this.databaseService.AddRepositoryFolderAsync(folder, ct);
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenDatabaseFolder))]
    private void OpenDatabaseFolder(string folder)
    {
        if (Directory.Exists(folder))
        {
            this.fileExplorerService.OpenInExplorer(folder);
        }
    }

    private bool CanOpenDatabaseFolder(string folder)
    {
        return Directory.Exists(folder);
    }

    [RelayCommand]
    private async Task RemoveDatabaseFolderAsync(string folder, CancellationToken ct)
    {
        if (folder is not null && this.DatabaseFolders.Contains(folder))
        {
            this.DatabaseFolders.Remove(folder);

            await this.databaseService.RemoveRepositoryFolderAsync(folder, ct);
        }
    }

    [RelayCommand]
    private async Task AddCueFolderAsync(CancellationToken ct)
    {
        var folder = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.CueFolder);
        if (folder != null)
        {
            if (this.CueFolders.Contains(folder))
            {
                return;
            }

            this.CueFolders.Add(folder);

            await this.databaseService.AddCueFolderAsync(folder, ct);
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenCueFolder))]
    private void OpenCueFolder(string folder)
    {
        if (Directory.Exists(folder))
        {
            this.fileExplorerService.OpenInExplorer(folder);
        }
    }

    private bool CanOpenCueFolder(string folder)
    {
        return Directory.Exists(folder);
    }

    [RelayCommand]
    private async Task RemoveCueFolderAsync(string folder, CancellationToken ct)
    {
        if (folder is not null && this.CueFolders.Contains(folder))
        {
            this.CueFolders.Remove(folder);

            await this.databaseService.RemoveCueFolderAsync(folder, ct);
        }
    }

    [RelayCommand]
    private async Task AddOfflineStorageFolderAsync(CancellationToken ct)
    {
        var folder = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.OfflineStorageFolder);
        if (folder != null)
        {
            if (this.OfflineStorageFolders.Contains(folder))
            {
                return;
            }

            this.OfflineStorageFolders.Add(folder);

            await this.offlineExplorerService.AddFolderAsync(folder, ct);
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenOfflineStorageFolder))]
    private void OpenOfflineStorageFolder(string folder)
    {
        if (Directory.Exists(folder))
        {
            this.fileExplorerService.OpenInExplorer(folder);
        }
    }

    private bool CanOpenOfflineStorageFolder(string folder)
    {
        return Directory.Exists(folder);
    }

    [RelayCommand]
    private async Task RemoveOfflineStorageFolderAsync(string folder, CancellationToken ct)
    {
        if (folder is not null && this.OfflineStorageFolders.Contains(folder))
        {
            this.OfflineStorageFolders.Remove(folder);

            await this.offlineExplorerService.RemoveFolderAsync(folder, ct);
        }
    }

    [RelayCommand]
    private async Task SwitchThemeAsync(ElementTheme theme)
    {
        if (this.ElementTheme != theme)
        {
            this.ElementTheme = theme;
            await this.themeSelectorService.SetThemeAsync(theme);
        }
    }

    [RelayCommand]
    private void SwitchExplorerFolderSortPriority(int priority)
    {
        this.SwitchExplorerFolderSortPriority((OfflineExplorerFolderSortPriority)priority);
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
        this.SwitchExplorerArchiveGrouping((OfflineExplorerArchiveGrouping)grouping);
    }

    private async Task LoadSettingsAsync(CancellationToken ct)
    {
        var repositoryFolders = await this.databaseService.GetRepositoryFoldersAsync(ct);
        var cueFolders = await this.databaseService.GetCueFoldersAsync(ct);
        var offlineFolders = await this.offlineExplorerService.GetFoldersAsync(ct);

        this.DatabaseFolders = [.. repositoryFolders.Where(f => !string.IsNullOrEmpty(f))];
        this.CueFolders = [.. cueFolders.Where(f => !string.IsNullOrEmpty(f))];
        this.OfflineStorageFolders = [.. offlineFolders.Where(f => !string.IsNullOrEmpty(f))];
    }

    private void SwitchExplorerArchiveGrouping(OfflineExplorerArchiveGrouping grouping)
    {
        if (this.ExplorerArchiveGrouping != grouping)
        {
            this.ExplorerArchiveGrouping = grouping;
            this.localSettingsService.SaveSetting(KnownSettingKeys.ExplorerArchiveGrouping, grouping.ToString());
        }
    }

    partial void OnExplorerShowArchivesInTreeChanged(bool value)
    {
        localSettingsService.SaveSetting(KnownSettingKeys.ExplorerShowArchiveInTree, value);
    }

    partial void OnRedumpUserChanged(string value)
    {
        if (!this.isLoadingSettings)
        {
            this.RedumpCredentialsStatus = new(string.Empty, StatusSeverity.None);

            localSettingsService.SaveSetting(KnownSettingKeys.RedumpUser, value);
        }
    }

    partial void OnRedumpPasswordChanged(string value)
    {
        if (!this.isLoadingSettings)
        {
            this.RedumpCredentialsStatus = new(string.Empty, StatusSeverity.None);

            localSettingsService.SaveEncryptedSetting(KnownSettingKeys.RedumpPassword, value);
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
