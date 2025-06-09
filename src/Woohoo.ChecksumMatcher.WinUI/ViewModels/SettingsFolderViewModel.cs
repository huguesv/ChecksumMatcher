// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Helpers;

public partial class SettingsFolderViewModel : ObservableObject, IDisposable
{
    private readonly IFileExplorerService fileExplorerService;
    private readonly IDispatcherQueue dispatcherQueue;
    private readonly IDispatcherQueueTimer debounceTimer;
    private readonly FileSystemWatcher? watcher;
    private readonly FileSystemWatcher? parentWatcher;
    private bool isDisposed;

    public SettingsFolderViewModel(IFileExplorerService fileExplorerService, IDispatcherQueue dispatcherQueue)
        : this(fileExplorerService, dispatcherQueue, string.Empty, null)
    {
    }

    public SettingsFolderViewModel(
        IFileExplorerService fileExplorerService,
        IDispatcherQueue dispatcherQueue,
        string path,
        IAsyncRelayCommand? removeCommand)
    {
        ArgumentNullException.ThrowIfNull(fileExplorerService);
        ArgumentNullException.ThrowIfNull(dispatcherQueue);
        ArgumentNullException.ThrowIfNull(path);

        this.fileExplorerService = fileExplorerService;
        this.dispatcherQueue = dispatcherQueue;
        this.Path = path;
        this.RemoveCommand = removeCommand;

        this.debounceTimer = this.dispatcherQueue.CreateTimer();

        if (Directory.Exists(this.Path))
        {
            this.watcher = new FileSystemWatcher(this.Path)
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName,
                EnableRaisingEvents = true,
                Filter = "*.*",
            };

            this.watcher.Created += this.OnFolderChange;
            this.watcher.Deleted += this.OnFolderChange;
        }

        var parentFolderPath = System.IO.Path.GetDirectoryName(this.Path);
        if (Directory.Exists(parentFolderPath))
        {
            this.parentWatcher = new FileSystemWatcher(parentFolderPath)
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName,
                EnableRaisingEvents = true,
                Filter = System.IO.Path.GetFileName(this.Path),
            };

            this.parentWatcher.Created += this.OnFolderChange;
            this.parentWatcher.Deleted += this.OnFolderChange;
            this.parentWatcher.Renamed += this.OnFolderChange;
        }
    }

    public IAsyncRelayCommand? RemoveCommand { get; init; }

    public string Path { get; }

    public string Details
    {
        get
        {
            if (!Directory.Exists(this.Path))
            {
                return Localized.FolderDetailsDoesNotExist;
            }

            int folderCount = this.GetFolderCount();
            int fileCount = this.GetFileCount();

            if (folderCount == 1 && fileCount == 1)
            {
                return string.Format(CultureInfo.CurrentUICulture, Localized.FolderDetailsOneFolderOneFileFormat, folderCount, fileCount);
            }
            else if (folderCount == 1 && fileCount != 1)
            {
                return string.Format(CultureInfo.CurrentUICulture, Localized.FolderDetailsOneFolderMultipleFilesFormat, folderCount, fileCount);
            }
            else if (folderCount != 1 && fileCount == 1)
            {
                return string.Format(CultureInfo.CurrentUICulture, Localized.FolderDetailsMultipleFoldersOneFileFormat, folderCount, fileCount);
            }
            else
            {
                return string.Format(CultureInfo.CurrentUICulture, Localized.FolderDetailsMultipleFoldersMultipleFilesFormat, folderCount, fileCount);
            }
        }
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                if (this.watcher is not null)
                {
                    this.watcher.Created -= this.OnFolderChange;
                    this.watcher.Deleted -= this.OnFolderChange;
                    this.watcher.Dispose();
                }

                if (this.parentWatcher is not null)
                {
                    this.parentWatcher.Created -= this.OnFolderChange;
                    this.parentWatcher.Deleted -= this.OnFolderChange;
                    this.parentWatcher.Renamed -= this.OnFolderChange;
                    this.parentWatcher.Dispose();
                }
            }

            this.isDisposed = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenInExplorer))]
    private void OpenInExplorer()
    {
        this.fileExplorerService.OpenInExplorer(this.Path);
    }

    private bool CanOpenInExplorer()
    {
        return Directory.Exists(this.Path);
    }

    private int GetFileCount()
    {
        try
        {
            return Directory.GetFiles(this.Path).Length;
        }
        catch
        {
            return 0;
        }
    }

    private int GetFolderCount()
    {
        try
        {
            return Directory.GetDirectories(this.Path).Length;
        }
        catch
        {
            return 0;
        }
    }

    private void OnFolderChange(object sender, FileSystemEventArgs args)
    {
        this.debounceTimer.Debounce(NotifyChanges, TimeSpan.FromMilliseconds(50), immediate: false);

        void NotifyChanges()
        {
            this.dispatcherQueue.TryEnqueue(() =>
            {
                this.OnPropertyChanged(nameof(this.Details));
            });
        }
    }
}
