// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

// TODO: See if this workaround is needed: https://github.com/files-community/Files/pull/15386
// sample: https://github.com/microsoft/Windows-universal-samples/tree/main/Samples/FilePicker/cs
// library: https://github.com/ghost1372/DevWinUI/tree/main/dev/DevWinUI/Common/Picker
internal class FilePickerService : IFilePickerService
{
    public async Task<string?> GetOpenFilePathAsync(string settingsIdentifier, params (string Name, string ExtensionList)[] filters)
    {
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

    public async Task<string?> GetOpenFolderPathAsync(string settingsIdentifier)
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

    public async Task<string?> GetSaveFilePathAsync(string settingsIdentifier, params (string Name, string ExtensionList)[] filters)
    {
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
}
