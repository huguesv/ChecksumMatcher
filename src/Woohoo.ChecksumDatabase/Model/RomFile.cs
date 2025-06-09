// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Model;

public sealed record class RomFile
{
    public RomFile(RomGame parentGame)
    {
        Requires.NotNull(parentGame);

        this.ParentGame = parentGame;
        this.Name = string.Empty;
        this.CRC32 = [];
        this.MD5 = [];
        this.SHA1 = [];
        this.SHA256 = [];
        this.Date = string.Empty;
        this.Merge = string.Empty;
        this.Status = RomStatus.Good;
    }

    public RomGame ParentGame { get; }

    public string Name { get; set; }

    public byte[] CRC32 { get; set; }

    public byte[] MD5 { get; set; }

    public byte[] SHA1 { get; set; }

    public byte[] SHA256 { get; set; }

    public long Size { get; set; }

    public string Date { get; set; }

    public string Merge { get; set; }

    public RomStatus Status { get; set; }
}
