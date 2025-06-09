// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumDatabase.Model;

public partial class DatabaseGameViewModel : ObservableObject
{
    private readonly RomGame game;
    private readonly IClipboardService clipboardService;

    [ObservableProperty]
    private DatabaseGameStatus status = DatabaseGameStatus.Unknown;

    [ObservableProperty]
    private bool isContainerWrongName = false;

    [ObservableProperty]
    private DatabaseRomViewModel? selectedRom;

    public DatabaseGameViewModel(RomGame game, IClipboardService clipboardService)
    {
        this.game = game;
        this.clipboardService = clipboardService;

        foreach (var rom in game.Roms)
        {
            var romViewModel = new DatabaseRomViewModel(rom, this.clipboardService);
            this.Roms.Add(romViewModel);
        }
    }

    public string Name => this.game.Name;

    public ObservableCollection<DatabaseRomViewModel> Roms { get; private set; } = [];

    [RelayCommand]
    public void CopyName()
    {
        this.clipboardService.SetText(this.Name);
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
