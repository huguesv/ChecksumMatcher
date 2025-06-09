// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public sealed partial class DatabaseGameViewModel : ObservableObject
{
    private readonly RomGame game;
    private readonly IClipboardService clipboardService;
    private readonly ILogger logger;

    public DatabaseGameViewModel(RomGame game, IClipboardService clipboardService, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(logger);

        this.game = game;
        this.clipboardService = clipboardService;
        this.logger = logger;

        List<DatabaseRomViewModel> viewModels = [];

        foreach (var rom in game.Roms)
        {
            var romViewModel = new DatabaseRomViewModel(rom, this.clipboardService, this.logger);
            viewModels.Add(romViewModel);
        }

        foreach (var disk in game.Disks)
        {
            var romViewModel = new DatabaseRomViewModel(disk, this.clipboardService, this.logger);
            viewModels.Add(romViewModel);
        }

        viewModels.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        this.Roms = new ObservableCollection<DatabaseRomViewModel>(viewModels);
    }

    [ObservableProperty]
    public partial DatabaseGameStatus Status { get; set; } = DatabaseGameStatus.Unknown;

    [ObservableProperty]
    public partial bool IsContainerWrongName { get; set; } = false;

    [ObservableProperty]
    public partial DatabaseRomViewModel? SelectedRom { get; set; }

    public string Name => this.game.Name;

    public ObservableCollection<DatabaseRomViewModel> Roms { get; private set; } = [];

    [RelayCommand]
    public void CopyToClipboard(object value)
    {
        try
        {
            this.clipboardService.SetText(value.ToString() ?? string.Empty);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    public void RefreshStatus()
    {
        if (this.Roms.Count == 0)
        {
            this.Status = DatabaseGameStatus.Complete;
        }
        else
        {
            bool anyFound = false;
            bool anyUnknown = false;
            bool anyMissing = false;
            bool anyIncorrectName = false;

            foreach (var rom in this.Roms)
            {
                if (rom.Status == DatabaseRomStatus.Unknown)
                {
                    anyUnknown = true;
                }
                else if (rom.Status == DatabaseRomStatus.NotFound)
                {
                    anyMissing = true;
                }
                else if (rom.Status == DatabaseRomStatus.FoundWrongName)
                {
                    anyIncorrectName = true;
                }
                else if (rom.Status == DatabaseRomStatus.Found)
                {
                    anyFound = true;
                }
            }

            if (anyUnknown)
            {
                this.Status = DatabaseGameStatus.Unknown;
            }
            else if (anyMissing)
            {
                if (!anyFound)
                {
                    this.Status = DatabaseGameStatus.Missing;
                }
                else if (anyIncorrectName || this.IsContainerWrongName)
                {
                    this.Status = DatabaseGameStatus.PartialIncorrectName;
                }
                else
                {
                    this.Status = DatabaseGameStatus.Partial;
                }
            }
            else
            {
                if (anyIncorrectName || this.IsContainerWrongName)
                {
                    this.Status = DatabaseGameStatus.CompleteIncorrectName;
                }
                else
                {
                    this.Status = DatabaseGameStatus.Complete;
                }
            }
        }
    }
}
