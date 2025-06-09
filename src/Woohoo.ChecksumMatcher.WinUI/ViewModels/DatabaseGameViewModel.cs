// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public partial class DatabaseGameViewModel : ObservableObject
{
    private readonly RomGame game;
    private readonly IClipboardService clipboardService;

    public DatabaseGameViewModel(RomGame game, IClipboardService clipboardService)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(clipboardService);

        this.game = game;
        this.clipboardService = clipboardService;

        foreach (var rom in game.Roms)
        {
            var romViewModel = new DatabaseRomViewModel(rom, this.clipboardService);
            this.Roms.Add(romViewModel);
        }
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
        this.clipboardService.SetText(value.ToString() ?? string.Empty);
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
