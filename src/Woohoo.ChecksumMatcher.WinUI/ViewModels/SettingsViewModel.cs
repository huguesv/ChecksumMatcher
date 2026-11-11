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
    private readonly IOperationCompletionService operationCompletionService;
    private readonly IDateTimeProviderService dateTimeProviderService;
    private readonly ILogger logger;
    private readonly IDispatcherQueue dispatcherQueue;

    private bool isLoadingSettings = false;

    public SettingsViewModel(
        IAboutInformationService aboutInformationService,
        IThemeSelectorService themeSelectorService,
        ILocalSettingsService localSettingsService,
        IConfirmationService confirmationService,
        IFilePickerService filePickerService,
        IFileExplorerService fileExplorerService,
        IDatabaseService databaseService,
        IOfflineExplorerService offlineExplorerService,
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
