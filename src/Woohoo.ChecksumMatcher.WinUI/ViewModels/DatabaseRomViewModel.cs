// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.Security.Cryptography;

public partial class DatabaseRomViewModel : ObservableObject
{
    private readonly RomFile rom;
    private readonly IClipboardService clipboardService;

    [ObservableProperty]
    private DatabaseRomStatus status = DatabaseRomStatus.Unknown;

    public DatabaseRomViewModel(RomFile rom, IClipboardService clipboardService)
    {
        this.rom = rom;
        this.clipboardService = clipboardService;
    }

    public string Name => this.rom.Name;

    public long Size => this.rom.Size;

    public RomStatus RomStatus => this.rom.Status;

    public string CRC32 => HashCalculator.HexToString(this.rom.CRC32);

    public string MD5 => HashCalculator.HexToString(this.rom.MD5);

    public string SHA1 => HashCalculator.HexToString(this.rom.SHA1);

    public string SHA256 => HashCalculator.HexToString(this.rom.SHA256);

    [RelayCommand]
    public void CopyName()
    {
        this.clipboardService.SetText(this.Name);
    }

    [RelayCommand]
    public void CopySize()
    {
        this.clipboardService.SetText(this.Size.ToString());
    }

    [RelayCommand]
    public void CopyCRC32()
    {
        this.clipboardService.SetText(this.CRC32.ToString());
    }

    [RelayCommand]
    public void CopyMD5()
    {
        this.clipboardService.SetText(this.MD5.ToString());
    }

    [RelayCommand]
    public void CopySHA1()
    {
        this.clipboardService.SetText(this.SHA1.ToString());
    }

    [RelayCommand]
    public void CopySHA256()
    {
        this.clipboardService.SetText(this.SHA256.ToString());
    }
}
