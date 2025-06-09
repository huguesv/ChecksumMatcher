// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.System;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Services;

public partial class CreateDatabaseViewModel : ObservableObject
{
    private readonly IFilePickerService filePickerService;
    private readonly ILocalSettingsService localSettingsService;
    private readonly DispatcherQueue dispatcherQueue;

    // TODO: convert to a service
    private readonly IDatabaseCreator creator;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDatabaseCommand))]
    private string sourceFolder = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDatabaseCommand))]
    private string databaseFilePath = string.Empty;

    [ObservableProperty]
    private string databaseName = string.Empty;

    [ObservableProperty]
    private string databaseAuthor = string.Empty;

    [ObservableProperty]
    private string databaseVersion = string.Empty;

    [ObservableProperty]
    private string databaseUrl = string.Empty;

    [ObservableProperty]
    private string status = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDatabaseCommand))]
    private bool isScanning;

    public CreateDatabaseViewModel(IFilePickerService filePickerService, ILocalSettingsService localSettingsService, IDispatcherQueueService dispatcherQueueService)
    {
        this.filePickerService = filePickerService;
        this.localSettingsService = localSettingsService;
        this.dispatcherQueue = dispatcherQueueService.GetWindowsDispatcher();
        this.creator = new DatabaseCreator();
        this.creator.Progress += (sender, args) =>
        {
            this.dispatcherQueue.TryEnqueue(() =>
            {
                switch (args.Status)
                {
                    case ScannerProgress.EnumeratingFilesStart:
                        this.Status = "Enumerating files";
                        break;
                    case ScannerProgress.EnumeratingFilesEnd:
                        break;
                    case ScannerProgress.CalculatingChecksumsStart:
                        this.Status = string.Format("Calculating checksums for {0}\\{1}", args.File?.ContainerAbsolutePath, args.File?.FileRelativePath);
                        break;
                    case ScannerProgress.CalculatingChecksumsEnd:
                        break;
                    case ScannerProgress.RebuildingRomStart:
                        break;
                    case ScannerProgress.RebuildingRomEnd:
                        break;
                    case ScannerProgress.Finished:
                        this.IsScanning = false;
                        this.Status = "Ready";
                        break;
                    case ScannerProgress.Canceled:
                        this.IsScanning = false;
                        this.Status = "Canceled";
                        break;
                    case ScannerProgress.Unused:
                        break;
                    case ScannerProgress.PerfectMatch:
                        break;
                    case ScannerProgress.IncorrectContainerOrFileName:
                        break;
                    case ScannerProgress.Missing:
                        break;
                    default:
                        break;
                }
            });
        };
    }

    [RelayCommand]
    public async Task BrowseSourceFolderAsync()
    {
        var folderPath = await this.filePickerService.GetOpenFolderPathAsync();
        if (!string.IsNullOrEmpty(folderPath))
        {
            this.SourceFolder = folderPath;
        }
    }

    [RelayCommand]
    public async Task BrowseDatabaseFileAsync()
    {
        var filePath = await this.filePickerService.GetSaveFilePathAsync();
        if (!string.IsNullOrEmpty(filePath))
        {
            this.DatabaseFilePath = filePath;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreateDatabase))]
    public void CreateDatabase()
    {
        if (!Directory.Exists(this.SourceFolder))
        {
            return;
        }

        this.IsScanning = true;

        var options = new DatabaseCreatorOptions
        {
            Author = this.DatabaseAuthor,
            Name = this.DatabaseName,
            Description = this.DatabaseName,
            Version = this.DatabaseVersion,
            Url = this.DatabaseUrl,
            ForceCalculateChecksums = true,
        };

        _ = Task.Run(() =>
        {
            this.creator.Build(this.SourceFolder, this.DatabaseFilePath, options);
        });
    }

    public bool CanCreateDatabase()
    {
        return !string.IsNullOrEmpty(this.SourceFolder) && !string.IsNullOrEmpty(this.DatabaseFilePath) && !this.IsScanning;
    }
}
