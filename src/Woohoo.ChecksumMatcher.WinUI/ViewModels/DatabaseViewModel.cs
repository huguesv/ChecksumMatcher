// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Collections;
using Windows.System;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Services;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.IO.AbstractFileSystem.Offline.Models;
using Woohoo.ChecksumMatcher.Core.Services;

public partial class DatabaseViewModel : ObservableRecipient
{
    private readonly RomDatabase db;
    private readonly IOfflineDiskFinderService offlineDiskFinderService;
    private readonly IClipboardService clipboardService;
    private readonly ILocalSettingsService localSettingsService;
    private readonly IFilePickerService filePickerService;
    private readonly DispatcherQueue dispatcherQueue;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    private ObservableCollection<ScanOnlineFolderItemViewModel> scanOnlineFolders = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    private ObservableCollection<ScanOfflineFolderItemViewModel> scanOfflineFolders = [];

    [ObservableProperty]
    private BrowseOfflineFolderViewModel browseOfflineFolder;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearScanResultsCommand))]
    private string scanStatus = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearScanResultsCommand))]
    [NotifyCanExecuteChangedFor(nameof(RebuildCommand))]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    private bool isScanning;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RebuildCommand))]
    private string rebuildSourceFolderPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RebuildCommand))]
    private string rebuildTargetFolderPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearRebuildResultsCommand))]
    private string rebuildStatus = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RebuildCommand))]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearRebuildResultsCommand))]
    private bool isRebuilding;

    [ObservableProperty]
    private bool rebuildRemoveSource = false;

    [ObservableProperty]
    private RebuildTargetContainerTypeViewModel? rebuildTargetContainerType;

    private DatabaseGameFilterKind filterKind = DatabaseGameFilterKind.All;

    [ObservableProperty]
    private string filterDescription = string.Empty;

    [ObservableProperty]
    private string romFilterDescription = string.Empty;

    [ObservableProperty]
    private RomStatus? romFilterStatus = null;

    [ObservableProperty]
    private string filterText = string.Empty;

    private long? filterTextNumeric = null;

    [ObservableProperty]
    private DatabaseGameViewModel? selectedGame;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    private bool useOnlineStorage = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    private bool useOfflineStorage = true;

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

    // TODO: convert to a service
    private readonly IScanner scanner;
    private readonly IRebuilder rebuilder;

    public DatabaseViewModel(RomDatabase db, IOfflineDiskFinderService offlineDiskFinderService, IClipboardService clipboardService, ILocalSettingsService localSettingsService, IFilePickerService filePickerService, DispatcherQueue dispatcherQueue)
    {
        this.db = db;
        this.offlineDiskFinderService = offlineDiskFinderService;
        this.clipboardService = clipboardService;
        this.localSettingsService = localSettingsService;
        this.filePickerService = filePickerService;
        this.dispatcherQueue = dispatcherQueue;

        this.scanner = new Scanner();
        this.scanner.Progress += this.Scanner_Progress;

        this.rebuilder = new Rebuilder();
        this.rebuilder.Progress += this.Rebuilder_Progress;

        foreach (var game in db.Games.OrderBy(g => g.Name))
        {
            var gameViewModel = new DatabaseGameViewModel(game, this.clipboardService);
            this.Games.Add(gameViewModel);
        }

        this.SummaryCount = db.Games.Count;
        this.SummaryFileCount = db.Games.Sum(g => g.Roms.Count);
        this.SummarySize = db.Games.Sum(g => g.Roms.Sum(r => r.Size));

        this.FilteredGames = new AdvancedCollectionView(this.Games, true)
        {
            Filter = (obj) =>
            {
                if (obj is DatabaseGameViewModel game)
                {
                    return (this.IsFilterNameMatch(game) || this.IsFilterSizeMatch(game) || this.IsFilterChecksumMatch(game)) && this.IsFilterStatusMatch(game) && this.IsRomFilterMatch(game);
                }
                return false;
            }
        };
        this.FilteredGames.ObserveFilterProperty(nameof(this.FilterText));
        this.FilteredGames.SortDescriptions.Add(new SortDescription(nameof(DatabaseGameViewModel.Name), SortDirection.Ascending));

        this.ForceExtractOnScan = this.localSettingsService.ReadSetting<bool>(SettingKeys.ForceExtractOnScan);

        this.UseOnlineStorage = this.localSettingsService.LoadDatabaseScopeSetting(this.Name, SettingKeys.UseOnlineFolders, true);
        this.ScanOnlineFolders = new ObservableCollection<ScanOnlineFolderItemViewModel>(this.localSettingsService.LoadDatabaseScopeCollectionSetting<OnlineScanFolderSetting>(this.Name, SettingKeys.ScanOnlineFolders).Select(sv => new ScanOnlineFolderItemViewModel(this) { FolderPath = sv.FolderPath, IsIncluded = sv.IsIncluded }));

        this.UseOfflineStorage = this.localSettingsService.LoadDatabaseScopeSetting(this.Name, SettingKeys.UseOfflineFolders, true);
        this.ScanOfflineFolders = new ObservableCollection<ScanOfflineFolderItemViewModel>(this.localSettingsService.LoadDatabaseScopeCollectionSetting<OfflineScanFolderSetting>(this.Name, SettingKeys.ScanOfflineFolders).Select(sv => new ScanOfflineFolderItemViewModel(this, sv.DiskName, sv.FolderPath) { IsIncluded = sv.IsIncluded }));

        this.BrowseOfflineFolder = new BrowseOfflineFolderViewModel(this.offlineDiskFinderService, this.localSettingsService, this.dispatcherQueue, this.ApplyBrowseOfflineFolderSelection);

        this.UpdateFilterDescription();
        this.UpdateRomFilterDescription();

        this.ScanSortedMatchedFiles = new AdvancedCollectionView(this.ScanMatchedFiles, true)
        {
            SortDescriptions = { new SortDescription(SortDirection.Ascending) }
        };

        this.ScanSortedUnusedFiles = new AdvancedCollectionView(this.ScanUnusedFiles, true)
        {
            SortDescriptions = { new SortDescription(SortDirection.Ascending) }
        };

        this.ScanSortedMissingFiles = new AdvancedCollectionView(this.ScanMissingFiles, true)
        {
            SortDescriptions = { new SortDescription(SortDirection.Ascending) }
        };

        this.ScanSortedIncorrectNameFiles = new AdvancedCollectionView(this.ScanIncorrectNameFiles, true)
        {
            SortDescriptions = { new SortDescription(SortDirection.Ascending) }
        };

        this.RebuildSortedMatchedFiles = new AdvancedCollectionView(this.RebuildMatchedFiles, true)
        {
            SortDescriptions = { new SortDescription(SortDirection.Ascending) }
        };

        this.RebuildSortedUnusedFiles = new AdvancedCollectionView(this.RebuildUnusedFiles, true)
        {
            SortDescriptions = { new SortDescription(SortDirection.Ascending) }
        };

        this.RebuildTargetContainerType = this.RebuildTargetContainerTypes.FirstOrDefault();
    }

    public ObservableCollection<RebuildTargetContainerTypeViewModel> RebuildTargetContainerTypes { get; } =
    [
        new() { Type = "folder", DisplayName = "Folder" },
        new() { Type = "torrentzip", DisplayName = "Zip" },
        new() { Type = "torrent7z", DisplayName = "7z" },
    ];

    public AdvancedCollectionView FilteredGames { get; }

    public ObservableCollection<DatabaseGameViewModel> Games { get; private set; } = [];

    public ObservableCollection<string> ScanMatchedFiles { get; private set; } = [];

    public AdvancedCollectionView ScanSortedMatchedFiles { get; }

    public ObservableCollection<string> ScanUnusedFiles { get; private set; } = [];

    public AdvancedCollectionView ScanSortedUnusedFiles { get; }

    public ObservableCollection<string> ScanMissingFiles { get; private set; } = [];

    public AdvancedCollectionView ScanSortedMissingFiles { get; }

    public ObservableCollection<string> ScanIncorrectNameFiles { get; private set; } = [];

    public AdvancedCollectionView ScanSortedIncorrectNameFiles { get; }

    public ObservableCollection<string> RebuildMatchedFiles { get; private set; } = [];

    public AdvancedCollectionView RebuildSortedMatchedFiles { get; }

    public ObservableCollection<string> RebuildUnusedFiles { get; private set; } = [];

    public AdvancedCollectionView RebuildSortedUnusedFiles { get; }

    public string Author => this.db.Author;

    public string Category => this.db.Category;

    public string Comment => this.db.Comment;

    public string Date => this.db.Date;

    public string Description => this.db.Description;

    public string Email => this.db.Email;

    public string Homepage => this.db.Homepage;

    public string Name => this.db.Name;

    public int SummaryCount { get; }

    public int SummaryFileCount { get; }

    public long SummarySize { get; }

    public string Url => this.db.Url;

    public string Version => this.db.Version;

    private void ApplyBrowseOfflineFolderSelection(OfflineDisk disk, string folderPath)
    {
        if (disk is null || string.IsNullOrEmpty(folderPath))
        {
            return;
        }

        var existing = this.ScanOfflineFolders.FirstOrDefault(f => f.FolderPath.Equals(folderPath, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            this.ScanOfflineFolders.Add(new ScanOfflineFolderItemViewModel(this, disk.Name, folderPath) { IsIncluded = true });
            this.ScanCommand.NotifyCanExecuteChanged();
            this.SaveScanOfflineFolderSetting();
        }
    }

    private bool IsFilterSizeMatch(DatabaseGameViewModel game)
    {
        return this.filterTextNumeric.HasValue && game.Roms.Any(r => r.Size == this.filterTextNumeric);
    }

    private bool IsFilterChecksumMatch(DatabaseGameViewModel game)
    {
        switch (this.FilterText.Length)
        {
            case 8:
                return game.Roms.Any(r => r.CRC32 == this.FilterText);
            case 32:
                return game.Roms.Any(r => r.MD5 == this.FilterText);
            case 40:
                return game.Roms.Any(r => r.SHA1 == this.FilterText);
            default:
                return false;
        }
    }

    private bool IsFilterNameMatch(DatabaseGameViewModel game)
    {
        return string.IsNullOrEmpty(this.FilterText) || game.Name.Contains(this.FilterText, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsRomFilterMatch(DatabaseGameViewModel game)
    {
        if (this.RomFilterStatus is null)
        {
            return true;
        }

        return game.Roms.Any(r => r.RomStatus == this.RomFilterStatus);
    }

    private bool IsFilterStatusMatch(DatabaseGameViewModel game)
    {
        return this.filterKind == DatabaseGameFilterKind.All ||
            (this.filterKind == DatabaseGameFilterKind.Complete && game.Status == DatabaseGameStatus.Complete) ||
            (this.filterKind == DatabaseGameFilterKind.Partial && game.Status == DatabaseGameStatus.Partial) ||
            (this.filterKind == DatabaseGameFilterKind.Missing && game.Status == DatabaseGameStatus.Missing) ||
            (this.filterKind == DatabaseGameFilterKind.WrongName && (game.Status == DatabaseGameStatus.PartialIncorrectName || game.Status == DatabaseGameStatus.CompleteIncorrectName));
    }

    partial void OnFilterTextChanged(string value)
    {
        if (long.TryParse(value, out long result))
        {
            this.filterTextNumeric = result;
        }
        else
        {
            this.filterTextNumeric = null;
        }

        this.FilteredGames.RefreshFilter();
    }

    [RelayCommand(CanExecute = nameof(CanCancelScan))]
    public void CancelScan()
    {
        this.scanner.Cancel();
    }

    [RelayCommand(CanExecute = nameof(CanClearScanResults))]
    public void ClearScanResults()
    {
        this.ScanStatus = string.Empty;

        this.ScanMatchedFiles.Clear();
        this.ScanMissingFiles.Clear();
        this.ScanUnusedFiles.Clear();
        this.ScanIncorrectNameFiles.Clear();

        foreach (var game in this.Games)
        {
            game.Status = DatabaseGameStatus.Unknown;
            game.IsContainerWrongName = false;

            foreach (var rom in game.Roms)
            {
                rom.Status = DatabaseRomStatus.Unknown;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanScan))]
    public void Scan()
    {
        if (!this.CanScan())
        {
            return;
        }

        this.ClearScanResults();

        Storage storage = new Storage();

        if (this.UseOnlineStorage)
        {
            foreach (var onlineFolder in this.ScanOnlineFolders.Where(sf => sf.IsIncluded && Directory.Exists(sf.FolderPath)))
            {
                storage.FolderPaths.Add(onlineFolder.FolderPath);
            }
        }

        if (this.UseOfflineStorage)
        {
            foreach (var offlineFolder in this.ScanOfflineFolders.Where(sf => sf.IsIncluded))
            {
                var selectedDisk = this.offlineDiskFinderService.TryLoadByName(offlineFolder.DiskName);
                if (selectedDisk?.Disk is not null)
                {
                    storage.OfflineFolders.Add(new OfflineStorage() { Disk = selectedDisk.Disk, FolderPath = offlineFolder.FolderPath });
                }
            }
        }

        var options = new ScanOptions()
        {
            ForceCalculateChecksums = this.localSettingsService.ReadSetting<bool>(SettingKeys.ForceExtractOnScan),
        };

        this.IsScanning = true;
        this.ScanStatus = "Starting scan...";

        _ = Task.Run(() =>
        {
            this.scanner.Scan(this.db, storage, options);
        });
    }

    public bool CanScan()
    {
        return
            !this.IsRebuilding &&
            !this.IsScanning &&
            (this.UseOnlineStorage && this.ScanOnlineFolders.Any(sf => sf.IsIncluded && Directory.Exists(sf.FolderPath)) || this.UseOfflineStorage && this.ScanOfflineFolders.Any(sf => sf.IsIncluded));
    }

    public bool CanCancelScan()
    {
        return this.IsScanning;
    }

    public bool CanClearScanResults()
    {
        return !this.IsScanning && !string.IsNullOrEmpty(this.ScanStatus);
    }

    [RelayCommand(CanExecute = nameof(CanCancelRebuild))]
    public void CancelRebuild()
    {
        this.rebuilder.Cancel();
    }

    public bool CanCancelRebuild()
    {
        return this.IsRebuilding;
    }

    [RelayCommand(CanExecute = nameof(CanClearRebuildResults))]
    public void ClearRebuildResults()
    {
        this.RebuildStatus = string.Empty;

        this.RebuildMatchedFiles.Clear();
        this.RebuildUnusedFiles.Clear();
    }

    public bool CanClearRebuildResults()
    {
        return !this.IsRebuilding && !string.IsNullOrEmpty(this.RebuildStatus);
    }

    [RelayCommand(CanExecute = nameof(CanRebuild))]
    public void Rebuild()
    {
        if (!this.CanRebuild())
        {
            return;
        }

        this.ClearRebuildResults();

        var options = new RebuildOptions()
        {
            ForceCalculateChecksums = this.localSettingsService.ReadSetting<bool>(SettingKeys.ForceExtractOnScan),
            RemoveSource = this.RebuildRemoveSource,
            TargetContainerType = this.RebuildTargetContainerType?.Type ?? "folder",
        };

        this.IsRebuilding = true;

        _ = Task.Run(() =>
        {
            this.rebuilder.Rebuild(this.db, this.RebuildSourceFolderPath, this.RebuildTargetFolderPath, options);
        });
    }

    public bool CanRebuild()
    {
        return
            !this.IsRebuilding &&
            !this.IsScanning &&
            Directory.Exists(this.RebuildSourceFolderPath) &&
            !string.IsNullOrEmpty(this.RebuildTargetFolderPath);
    }

    public void RemoveScanFolder(ScanOnlineFolderItemViewModel folder)
    {
        if (folder is not null && this.ScanOnlineFolders.Contains(folder))
        {
            this.ScanOnlineFolders.Remove(folder);
            this.ScanCommand.NotifyCanExecuteChanged();
            this.SaveScanOnlineFolderSetting();
        }
    }

    public void RemoveScanFolder(ScanOfflineFolderItemViewModel folder)
    {
        if (folder is not null && this.ScanOfflineFolders.Contains(folder))
        {
            this.ScanOfflineFolders.Remove(folder);
            this.ScanCommand.NotifyCanExecuteChanged();
            this.SaveScanOfflineFolderSetting();
        }
    }


    [RelayCommand]
    public async Task AddScanOnlineFolderAsync()
    {
        var folderPath = await this.filePickerService.GetOpenFolderPathAsync();
        if (!string.IsNullOrEmpty(folderPath))
        {
            var existing = this.ScanOnlineFolders.FirstOrDefault(f => f.FolderPath.Equals(folderPath, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                this.ScanOnlineFolders.Add(new ScanOnlineFolderItemViewModel(this) { FolderPath = folderPath, IsIncluded = true });
                this.ScanCommand.NotifyCanExecuteChanged();
                this.SaveScanOnlineFolderSetting();
            }
        }
    }

    [RelayCommand]
    public async Task BrowseRebuildSourceFolderAsync()
    {
        var folderPath = await this.filePickerService.GetOpenFolderPathAsync();
        if (!string.IsNullOrEmpty(folderPath))
        {
            this.RebuildSourceFolderPath = folderPath;
        }
    }

    [RelayCommand]
    public async Task BrowseRebuildTargetFolderAsync()
    {
        var folderPath = await this.filePickerService.GetOpenFolderPathAsync();
        if (!string.IsNullOrEmpty(folderPath))
        {
            this.RebuildTargetFolderPath = folderPath;
        }
    }

    [RelayCommand]
    public void CopyHeaderInfo()
    {
        var current = new StringBuilder();

        if (!string.IsNullOrEmpty(this.db.Name))
        {
            current.AppendLine(Localized.HeaderName);
            current.AppendLine(this.db.Name);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.db.Description))
        {
            current.AppendLine(Localized.HeaderDescription);
            current.AppendLine(this.db.Description);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.db.Category))
        {
            current.AppendLine(Localized.HeaderCategory);
            current.AppendLine(this.db.Category);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.db.Version))
        {
            current.AppendLine(Localized.HeaderVersion);
            current.AppendLine(this.db.Version);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.db.Date))
        {
            current.AppendLine(Localized.HeaderDate);
            current.AppendLine(this.db.Date);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.db.Author))
        {
            current.AppendLine(Localized.HeaderAuthor);
            current.AppendLine(this.db.Author);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.db.Email))
        {
            current.AppendLine(Localized.HeaderEmail);
            current.AppendLine(this.db.Email);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.db.Homepage))
        {
            current.AppendLine(Localized.HeaderHomepage);
            current.AppendLine(this.db.Homepage);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.db.Url))
        {
            current.AppendLine(Localized.HeaderUrl);
            current.AppendLine(this.db.Url);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.db.Comment))
        {
            current.AppendLine(Localized.HeaderComment);
            current.AppendLine(this.db.Comment);
            current.AppendLine();
        }

        this.clipboardService.SetText(current.ToString());
    }

    [RelayCommand]
    public void CopyHeaderStats()
    {
        var current = new StringBuilder();

        current.AppendLine(Localized.StatisticContainerCount);
        current.AppendLine(this.SummaryCount.ToString());
        current.AppendLine();

        current.AppendLine(Localized.StatisticFileCount);
        current.AppendLine(this.SummaryFileCount.ToString());
        current.AppendLine();

        current.AppendLine(Localized.StatisticFileSizeTotal);
        current.AppendLine(this.SummarySize.ToString());
        current.AppendLine();

        this.clipboardService.SetText(current.ToString());
    }

    [RelayCommand]
    public void FilterAll()
    {
        this.filterKind = DatabaseGameFilterKind.All;
        this.UpdateFilterDescription();
        this.FilteredGames.RefreshFilter();
    }

    [RelayCommand]
    public void FilterComplete()
    {
        this.filterKind = DatabaseGameFilterKind.Complete;
        this.UpdateFilterDescription();
        this.FilteredGames.RefreshFilter();
    }

    [RelayCommand]
    public void FilterMissing()
    {
        this.filterKind = DatabaseGameFilterKind.Missing;
        this.UpdateFilterDescription();
        this.FilteredGames.RefreshFilter();
    }

    [RelayCommand]
    public void FilterPartial()
    {
        this.filterKind = DatabaseGameFilterKind.Partial;
        this.UpdateFilterDescription();
        this.FilteredGames.RefreshFilter();
    }

    [RelayCommand]
    public void FilterWrongName()
    {
        this.filterKind = DatabaseGameFilterKind.WrongName;
        this.UpdateFilterDescription();
        this.FilteredGames.RefreshFilter();
    }

    [RelayCommand]
    public void FilterRomStatus(string status)
    {
        switch (status)
        {
            case "all":
                this.RomFilterStatus = null;
                break;

            case "good":
                this.RomFilterStatus = RomStatus.Good;
                break;

            case "bad":
                this.RomFilterStatus = RomStatus.BadDump;
                break;

            case "verified":
                this.RomFilterStatus = RomStatus.Verified;
                break;

            case "nodump":
                this.RomFilterStatus = RomStatus.NoDump;
                break;
        }

        this.UpdateRomFilterDescription();
        this.FilteredGames.RefreshFilter();
    }

    private void SaveScanOnlineFolderSetting()
    {
        this.localSettingsService.SaveDatabaseScopeCollectionSetting<OnlineScanFolderSetting>(
            this.Name,
            SettingKeys.ScanOnlineFolders,
            this.ScanOnlineFolders.Select(sf => new OnlineScanFolderSetting { FolderPath = sf.FolderPath, IsIncluded = sf.IsIncluded }).ToArray());
    }

    private void SaveScanOfflineFolderSetting()
    {
        this.localSettingsService.SaveDatabaseScopeCollectionSetting<OfflineScanFolderSetting>(
            this.Name,
            SettingKeys.ScanOfflineFolders,
            this.ScanOfflineFolders.Select(sf => new OfflineScanFolderSetting { DiskName = sf.DiskName, FolderPath = sf.FolderPath, IsIncluded = sf.IsIncluded }).ToArray());
    }

    private void Scanner_Progress(object? sender, ScannerProgressEventArgs e)
    {
        this.dispatcherQueue.TryEnqueue(() =>
        {
            switch (e.Status)
            {
                case ScannerProgress.EnumeratingFilesStart:
                    this.ScanStatus = "Enumerating files";
                    break;
                case ScannerProgress.EnumeratingFilesEnd:
                    this.ScanStatus = "Scanning files";
                    break;
                case ScannerProgress.CalculatingChecksumsStart:
                    this.ScanStatus = string.Format("Calculating checksums for {0}\\{1}", e.File?.ContainerAbsolutePath, e.File?.FileRelativePath);
                    break;
                case ScannerProgress.CalculatingChecksumsEnd:
                    this.ScanStatus = "Scanning files";
                    break;
                case ScannerProgress.Finished:
                    {
                        if (e.Result is not null)
                        {
                            foreach (var unusedFile in e.Result.UnusedFiles)
                            {
                                this.ScanUnusedFiles.Add(unusedFile.File.ContainerName + "\\" + unusedFile.File.FileRelativePath);
                            }
                        }

                        this.IsScanning = false;
                        this.ScanStatus = $"Scan complete at {DateTime.Now}";
                        this.FilteredGames.RefreshFilter();
                    }
                    break;
                case ScannerProgress.Canceled:
                    this.IsScanning = false;
                    this.ScanStatus = "Canceled";
                    break;
                case ScannerProgress.PerfectMatch:
                    {
                        var game = this.Games.SingleOrDefault(temp => temp.Name == e.Rom?.ParentGame.Name);
                        if (game is not null)
                        {
                            var rom = game.Roms.SingleOrDefault(temp => temp.Name == e.Rom?.Name);
                            if (rom is not null)
                            {
                                rom.Status = DatabaseRomStatus.Found;
                            }

                            game.RefreshStatus();
                        }

                        this.ScanMatchedFiles.Add(e.Rom?.ParentGame.Name + "\\" + e.Rom?.Name);
                    }
                    break;
                case ScannerProgress.IncorrectContainerOrFileName:
                    {
                        var game = this.Games.SingleOrDefault(temp => temp.Name == e.Rom?.ParentGame.Name);
                        if (game is not null && e.File is not null)
                        {
                            var rom = game.Roms.SingleOrDefault(temp => temp.Name == e.Rom?.Name);
                            if (rom is not null)
                            {
                                if (e.File.FileRelativePath != e.Rom?.Name)
                                {
                                    rom.Status = DatabaseRomStatus.FoundWrongName;
                                }
                                else
                                {
                                    rom.Status = DatabaseRomStatus.Found;
                                }
                            }

                            if (e.File.ContainerName != e.Rom?.ParentGame.Name)
                            {
                                game.IsContainerWrongName = true;
                            }

                            game.RefreshStatus();

                            this.ScanIncorrectNameFiles.Add(e.File.ContainerName + "\\" + e.File.FileRelativePath + " >>> " + e.Rom?.ParentGame.Name + "\\" + e.Rom?.Name);
                        }
                    }
                    break;
                case ScannerProgress.Missing:
                    {
                        var game = this.Games.SingleOrDefault(temp => temp.Name == e.Rom?.ParentGame.Name);
                        if (game is not null)
                        {
                            var rom = game.Roms.SingleOrDefault(temp => temp.Name == e.Rom?.Name);
                            if (rom is not null)
                            {
                                rom.Status = DatabaseRomStatus.NotFound;
                            }

                            game.RefreshStatus();
                        }

                        this.ScanMissingFiles.Add(e.Rom?.ParentGame.Name + "\\" + e.Rom?.Name);
                    }
                    break;
                default:
                    break;
            }
        });
    }

    private void Rebuilder_Progress(object? sender, RebuilderProgressEventArgs e)
    {
        this.dispatcherQueue.TryEnqueue(() =>
        {
            switch (e.Status)
            {
                case ScannerProgress.EnumeratingFilesStart:
                    this.RebuildStatus = "Enumerating files";
                    break;
                case ScannerProgress.EnumeratingFilesEnd:
                    this.RebuildStatus = "Scanning files";
                    break;
                case ScannerProgress.CalculatingChecksumsStart:
                    this.RebuildStatus = string.Format("Calculating checksums for {0}\\{1}", e.File?.ContainerAbsolutePath, e.File?.FileRelativePath);
                    break;
                case ScannerProgress.CalculatingChecksumsEnd:
                    this.RebuildStatus = "Scanning files";
                    break;
                case ScannerProgress.RebuildingRomStart:
                    this.RebuildStatus = $"Rebuilding {e.Rom?.ParentGame.Name}\\{e.Rom?.Name} ...";
                    break;
                case ScannerProgress.RebuildingRomEnd:
                    this.RebuildStatus = "Scanning files";
                    this.RebuildMatchedFiles.Add(e.Rom?.ParentGame.Name + "\\" + e.Rom?.Name);
                    break;
                case ScannerProgress.Finished:
                    this.IsRebuilding = false;
                    this.RebuildStatus = $"Rebuild complete at {DateTime.Now}";
                    this.FilteredGames.RefreshFilter();
                    break;
                case ScannerProgress.Canceled:
                    this.IsRebuilding = false;
                    this.RebuildStatus = "Canceled";
                    break;
                case ScannerProgress.Unused:
                    this.RebuildStatus = "Scanning files";
                    this.RebuildUnusedFiles.Add(e.File?.ContainerName + "\\" + e.File?.FileRelativePath);
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
    }

    private void UpdateFilterDescription()
    {
        switch (this.filterKind)
        {
            case DatabaseGameFilterKind.All:
                this.FilterDescription = "All";
                break;
            case DatabaseGameFilterKind.Missing:
                this.FilterDescription = "Missing";
                break;
            case DatabaseGameFilterKind.Complete:
                this.FilterDescription = "Complete";
                break;
            case DatabaseGameFilterKind.Partial:
                this.FilterDescription = "Partial";
                break;
            case DatabaseGameFilterKind.WrongName:
                this.FilterDescription = "Wrong Name";
                break;
            case DatabaseGameFilterKind.Unknown:
                this.FilterDescription = "Unknown";
                break;
            default:
                break;
        }
    }

    private void UpdateRomFilterDescription()
    {
        if (this.RomFilterStatus is null)
        {
            this.RomFilterDescription = "All";
            return;
        }

        switch (this.RomFilterStatus.Value)
        {
            case RomStatus.BadDump:
                this.RomFilterDescription = "Bad";
                break;
            case RomStatus.NoDump:
                this.RomFilterDescription = "NoDump";
                break;
            case RomStatus.Good:
                this.RomFilterDescription = "Good";
                break;
            case RomStatus.Verified:
                this.RomFilterDescription = "Verified";
                break;
            default:
                break;
        }
    }

    internal void OnIsIncludedChanged(ScanOnlineFolderItemViewModel onlineFolderItemViewModel, bool value)
    {
        this.ScanCommand.NotifyCanExecuteChanged();
        this.SaveScanOnlineFolderSetting();
    }

    internal void OnIsIncludedChanged(ScanOfflineFolderItemViewModel offlineFolderItemViewModel, bool value)
    {
        this.ScanCommand.NotifyCanExecuteChanged();
        this.SaveScanOfflineFolderSetting();
    }

    partial void OnUseOfflineStorageChanged(bool value)
    {
        this.localSettingsService.SaveDatabaseScopeSetting(this.Name, SettingKeys.UseOfflineFolders, value);
    }

    partial void OnUseOnlineStorageChanged(bool value)
    {
        this.localSettingsService.SaveDatabaseScopeSetting(this.Name, SettingKeys.UseOnlineFolders, value);
    }
}
