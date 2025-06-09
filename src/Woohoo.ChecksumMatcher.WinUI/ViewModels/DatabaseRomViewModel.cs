// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.Security.Cryptography;

public sealed partial class DatabaseRomViewModel : ObservableObject
{
    private readonly RomFile? rom;
    private readonly RomDisk? disk;
    private readonly IClipboardService clipboardService;
    private readonly ILogger logger;

    public DatabaseRomViewModel(RomFile rom, IClipboardService clipboardService, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(rom);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(logger);

        this.rom = rom;
        this.clipboardService = clipboardService;
        this.logger = logger;
    }

    public DatabaseRomViewModel(RomDisk disk, IClipboardService clipboardService, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(disk);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(logger);

        this.disk = disk;
        this.clipboardService = clipboardService;
        this.logger = logger;
    }

    [ObservableProperty]
    public partial DatabaseRomStatus Status { get; set; } = DatabaseRomStatus.Unknown;

    public string Name
    {
        get
        {
            if (this.rom != null)
            {
                return this.rom.Name;
            }
            else if (this.disk != null)
            {
                return this.disk.Name + ".chd";
            }

            return string.Empty;
        }
    }

    public long? Size => this.rom?.Size;

    public string SizeAsText => this.rom?.Size.ToString() ?? string.Empty;

    public RomStatus RomStatus => this.rom?.Status ?? this.disk?.Status ?? RomStatus.Good;

    public string CRC32 => HashCalculator.HexToString(this.rom?.CRC32 ?? []);

    public string MD5 => HashCalculator.HexToString(this.rom?.MD5 ?? []);

    public string SHA1 => HashCalculator.HexToString(this.rom?.SHA1 ?? this.disk?.SHA1 ?? []);

    public string SHA256 => HashCalculator.HexToString(this.rom?.SHA256 ?? []);

    [RelayCommand]
    private void CopyToClipboard(object value)
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
}
