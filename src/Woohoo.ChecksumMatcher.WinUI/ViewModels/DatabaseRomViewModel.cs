// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.Security.Cryptography;

public sealed partial class DatabaseRomViewModel : ObservableObject
{
    private readonly RomFile rom;
    private readonly IClipboardService clipboardService;

    public DatabaseRomViewModel(RomFile rom, IClipboardService clipboardService)
    {
        ArgumentNullException.ThrowIfNull(rom);
        ArgumentNullException.ThrowIfNull(clipboardService);

        this.rom = rom;
        this.clipboardService = clipboardService;
    }

    [ObservableProperty]
    public partial DatabaseRomStatus Status { get; set; } = DatabaseRomStatus.Unknown;

    public string Name => this.rom.Name;

    public long Size => this.rom.Size;

    public RomStatus RomStatus => this.rom.Status;

    public string CRC32 => HashCalculator.HexToString(this.rom.CRC32);

    public string MD5 => HashCalculator.HexToString(this.rom.MD5);

    public string SHA1 => HashCalculator.HexToString(this.rom.SHA1);

    public string SHA256 => HashCalculator.HexToString(this.rom.SHA256);

    [RelayCommand]
    private void CopyToClipboard(object value)
    {
        this.clipboardService.SetText(value.ToString() ?? string.Empty);
    }
}
