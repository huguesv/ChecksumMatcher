// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Helpers;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService themeSelectorService;
    private readonly ILocalSettingsService localSettingsService;
    private readonly IFilePickerService filePickerService;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    [ObservableProperty]
    private string _version;

    public ObservableCollection<string> DatabaseFolders { get; private set; } = [];

    public ObservableCollection<string> IndexFolders { get; private set; } = [];

    private bool _forceExtractOnScan;

    public bool ForceExtractOnScan
    {
        get => _forceExtractOnScan;
        set
        {
            if (this.SetProperty(ref this._forceExtractOnScan, value))
            {
                localSettingsService.SaveSetting(SettingKeys.ForceExtractOnScan, value);
            }
        }
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, ILocalSettingsService localSettingsService, IFilePickerService filePickerService)
    {
        this.themeSelectorService = themeSelectorService;
        this.localSettingsService = localSettingsService;
        this.filePickerService = filePickerService;

        _elementTheme = this.themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();
        _version = GetVersion();

        _forceExtractOnScan = this.localSettingsService.ReadSetting<bool>(SettingKeys.ForceExtractOnScan);

        var databaseFolders = this.localSettingsService.ReadSetting<string[]>(SettingKeys.DatabaseFolders);
        if (databaseFolders is not null)
        {
            foreach (var folder in databaseFolders)
            {
                if (!string.IsNullOrEmpty(folder))
                {
                    this.DatabaseFolders.Add(folder);
                }
            }
        }

        var indexFolders = this.localSettingsService.ReadSetting<string[]>(SettingKeys.OfflineFolders);
        if (indexFolders is not null)
        {
            foreach (var folder in indexFolders)
            {
                if (!string.IsNullOrEmpty(folder))
                {
                    this.IndexFolders.Add(folder);
                }
            }
        }
    }

    [RelayCommand]
    private async Task AddDatabaseFolderAsync()
    {
        var folder = await this.filePickerService.GetOpenFolderPathAsync();
        if (folder != null)
        {
            this.DatabaseFolders.Add(folder);
            localSettingsService.SaveSetting(SettingKeys.DatabaseFolders, this.DatabaseFolders.ToArray());
        }
    }

    [RelayCommand]
    private void RemoveDatabaseFolder(string folder)
    {
        if (folder is not null && this.DatabaseFolders.Contains(folder))
        {
            this.DatabaseFolders.Remove(folder);
            localSettingsService.SaveSetting(SettingKeys.DatabaseFolders, this.DatabaseFolders.ToArray());
        }
    }

    [RelayCommand]
    private async Task AddIndexFolderAsync()
    {
        var folder = await this.filePickerService.GetOpenFolderPathAsync();
        if (folder != null)
        {
            this.IndexFolders.Add(folder);
            localSettingsService.SaveSetting(SettingKeys.OfflineFolders, this.IndexFolders.ToArray());
        }
    }

    [RelayCommand]
    private void RemoveIndexFolder(string folder)
    {
        if (folder is not null && this.IndexFolders.Contains(folder))
        {
            this.IndexFolders.Remove(folder);
            localSettingsService.SaveSetting(SettingKeys.OfflineFolders, this.IndexFolders.ToArray());
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

        return $"Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    private static string GetVersionDescription()
    {
        return $"{"AppDisplayName".GetLocalized()} - {GetVersion()}";
    }
}
