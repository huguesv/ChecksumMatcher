// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Models;
using Woohoo.ChecksumMatcher.WinUI.Views;

internal sealed class FilePickerService : IFilePickerService
{
    private readonly ILocalSettingsService localSettingsService;

    public FilePickerService(ILocalSettingsService localSettingsService)
    {
        ArgumentNullException.ThrowIfNull(localSettingsService);

        this.localSettingsService = localSettingsService;
    }

    public async Task<string?> GetOpenFilePathAsync(string settingsIdentifier, params (string Name, string ExtensionList)[] filters)
    {
        ArgumentNullException.ThrowIfNull(settingsIdentifier);
        ArgumentNullException.ThrowIfNull(filters);

        var openPicker = new FileOpenPicker();

        var window = App.MainWindow;

        var hWnd = WindowNative.GetWindowHandle(window);

        InitializeWithWindow.Initialize(openPicker, hWnd);

        openPicker.SettingsIdentifier = settingsIdentifier;
        openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
        openPicker.ViewMode = PickerViewMode.List;
        if (filters.Length == 0)
        {
            openPicker.FileTypeFilter.Add("*"); // Allow all file types
        }
        else
        {
            foreach (var filter in filters)
            {
                openPicker.FileTypeFilter.Add(filter.ExtensionList);
            }
        }

        StorageFile file = await openPicker.PickSingleFileAsync();
        if (file is not null)
        {
            return file.Path;
        }

        return null;
    }

    public Task<string?> GetOpenFolderPathAsync(string settingsIdentifier)
    {
        ArgumentNullException.ThrowIfNull(settingsIdentifier);

        bool useWinUIFolderPicker = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.UseWinUIFolderPicker) ?? false;
        if (useWinUIFolderPicker)
        {
            return this.GetOpenFolderPathWinUIAsync(settingsIdentifier);
        }
        else
        {
            return this.GetOpenFolderPathWin32Async(settingsIdentifier);
        }
    }

    public async Task<string?> GetSaveFilePathAsync(string settingsIdentifier, params (string Name, string ExtensionList)[] filters)
    {
        ArgumentNullException.ThrowIfNull(settingsIdentifier);
        ArgumentNullException.ThrowIfNull(filters);

        if (filters.Length == 0)
        {
            throw new ArgumentException("At least one filter must be provided.", nameof(filters));
        }

        var savePicker = new FileSavePicker();

        var window = App.MainWindow;

        var hWnd = WindowNative.GetWindowHandle(window);

        InitializeWithWindow.Initialize(savePicker, hWnd);

        savePicker.SettingsIdentifier = settingsIdentifier;
        savePicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

        foreach (var filter in filters)
        {
            savePicker.FileTypeChoices.Add(filter.Name, filter.ExtensionList.Split(';'));
        }

        StorageFile file = await savePicker.PickSaveFileAsync();
        if (file is not null)
        {
            return file.Path;
        }

        return null;
    }

    public async Task<OfflineDiskFolder?> GetOfflineDiskFolderAsync()
    {
        XamlRoot? xamlRoot = (App.MainWindow.Content as FrameworkElement)?.XamlRoot
            ?? throw new InvalidOperationException("Could not find xaml root.");

        var dialog = new OfflineStorageFolderSelectionDialog();

        await dialog.ShowAsync(xamlRoot);

        return dialog.SelectedDiskFolder;
    }

    private Task<string?> GetOpenFolderPathWin32Async(string settingsIdentifier)
    {
        var openPicker = new Woohoo.WinUI.Pickers.FolderPicker(App.MainWindow);

        var locationsMap = this.localSettingsService.ReadSetting<Dictionary<string, string>>(KnownSettingKeys.Win32FolderPickerLastLocations) ?? [];
        if (locationsMap.TryGetValue(settingsIdentifier, out var lastLocation))
        {
            if (Directory.Exists(lastLocation))
            {
                openPicker.InitialDirectory = lastLocation;
            }
        }

        var result = openPicker.PickSingleFolder();
        if (result is not null)
        {
            // Save the selected folder path in the last locations map
            locationsMap[settingsIdentifier] = result;
            this.localSettingsService.SaveSetting(KnownSettingKeys.Win32FolderPickerLastLocations, locationsMap);
        }

        return Task.FromResult(result);
    }

    private async Task<string?> GetOpenFolderPathWinUIAsync(string settingsIdentifier)
    {
        // Create a folder picker
        var openPicker = new FolderPicker();

        // See the sample code below for how to make the window accessible from the App class.
        var window = App.MainWindow;

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WindowNative.GetWindowHandle(window);

        // Initialize the folder picker with the window handle (HWND).
        InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your folder picker
        openPicker.SettingsIdentifier = settingsIdentifier;
        openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
        openPicker.FileTypeFilter.Add("*");
        openPicker.ViewMode = PickerViewMode.List;

        // Open the picker for the user to pick a folder
        StorageFolder folder = await openPicker.PickSingleFolderAsync();
        if (folder is not null)
        {
            // StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            return folder.Path;
        }

        return null;
    }
}
