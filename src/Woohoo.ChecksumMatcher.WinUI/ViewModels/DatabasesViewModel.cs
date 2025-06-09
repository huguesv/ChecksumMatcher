// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.System;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.ViewModels;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;

public partial class DatabasesViewModel : ObservableRecipient, INavigationAware
{
    private readonly ILocalSettingsService localSettingsService;
    private readonly IDatabaseFinderService databaseFinderService;
    private readonly IOfflineDiskFinderService offlineDiskFinderService;
    private readonly IClipboardService clipboardService;
    private readonly IFilePickerService filePickerService;
    private readonly DispatcherQueue dispatcherQueue;

    [ObservableProperty]
    private DatabaseTreeItemViewModel? selected;

    public ObservableCollection<DatabaseTreeItemViewModel> DataSource { get; } = [];

    public DatabasesViewModel(ILocalSettingsService localSettingsService, IDatabaseFinderService databaseFinderService, IOfflineDiskFinderService offlineDiskFinderService, IClipboardService clipboardService, IFilePickerService filePickerService, IDispatcherQueueService dispatcherQueueService)
    {
        this.localSettingsService = localSettingsService;
        this.databaseFinderService = databaseFinderService;
        this.offlineDiskFinderService = offlineDiskFinderService;
        this.clipboardService = clipboardService;
        this.filePickerService = filePickerService;
        this.dispatcherQueue = dispatcherQueueService.GetWindowsDispatcher();

        var results = this.databaseFinderService.FindDatabases();
        foreach (var result in results.OrderBy(d => d.RelativeFilePath))
        {
            this.AddResult(result);
        }
    }

    private void AddResult(DatabaseFindResult result)
    {
        var folderPath = Path.Combine(new DirectoryInfo(Path.GetFileName(result.RootFolder)).Name, Path.GetDirectoryName(result.RelativeFilePath)!);
        var folderItem = this.GetOrCreateFolderItem(folderPath);

        var databaseItem = this.CreateDatabaseItem(folderItem, result);
        folderItem.Children.Add(databaseItem);
    }

    private DatabaseTreeItemViewModel CreateDatabaseItem(DatabaseFolderTreeItemViewModel parent, DatabaseFindResult result)
    {
        DatabaseViewModel? databaseViewModel = null;
        if (result.Database is not null)
        {
            databaseViewModel = new DatabaseViewModel(result.Database, this.offlineDiskFinderService, this.clipboardService, this.localSettingsService, this.filePickerService, this.dispatcherQueue);
        }

        return new DatabaseFileTreeItemViewModel(parent, result.Database?.Name ?? string.Empty, result.RootFolder, result.AbsoluteFilePath, result.RelativeFilePath, databaseViewModel);
    }

    private DatabaseFolderTreeItemViewModel GetOrCreateFolderItem(string folder)
    {
        DatabaseFolderTreeItemViewModel? currentParent = null;

        var nameParts = folder.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        foreach (var namePart in nameParts)
        {
            var currentParentCollection = currentParent?.Children ?? this.DataSource;

            var existingFolder = currentParentCollection.FirstOrDefault(item => item is DatabaseFolderTreeItemViewModel folderItem && folderItem.Name == namePart) as DatabaseFolderTreeItemViewModel;
            if (existingFolder is null)
            {
                var newFolder = new DatabaseFolderTreeItemViewModel(namePart);
                currentParentCollection.Add(newFolder);
                currentParent = newFolder;
            }
            else
            {
                currentParent = existingFolder;
            }
        }

        return currentParent ?? throw new InvalidOperationException("Failed to create or find the folder item.");
    }

    public void OnNavigatedTo(object parameter)
    {
        //this.SampleItems.Clear();

        //// TODO: Replace with real data.
        //var data = await _sampleDataService.GetListDetailsDataAsync();

        //foreach (var item in data)
        //{
        //    this.SampleItems.Add(item);
        //}
    }

    public void OnNavigatedFrom()
    {
    }

    public void EnsureItemSelected()
    {
        //this.Selected ??= this.SampleItems.First();
    }

    partial void OnSelectedChanged(DatabaseTreeItemViewModel? value)
    {
        if (value is DatabaseFileTreeItemViewModel item && item.Database is null)
        {
            this.ReloadDatabaseItemAsync(item).SafeFireAndForget();
        }
    }

    private async Task ReloadDatabaseItemAsync(DatabaseFileTreeItemViewModel item)
    {
        var result = await Task.Run(() => this.databaseFinderService.LoadDatabase(item.RootFolder, item.AbsoluteFilePath));
        if (result is not null)
        {
            var parent = item.ParentFolderItem;
            var parentExpanded = parent.IsExpanded;

            var index = item.ParentFolderItem.Children.IndexOf(item);
            parent.Children.RemoveAt(index);

            var databaseItem = this.CreateDatabaseItem(parent, result);
            parent.Children.Insert(index, databaseItem);
            parent.IsExpanded = parentExpanded;

            // Removing the old item above sets the Selected property to null.
            // Set it again unless user has already selected a different item.
            if (this.Selected is null || this.Selected == item)
            {
                this.Selected = databaseItem;
            }
        }
    }
}
