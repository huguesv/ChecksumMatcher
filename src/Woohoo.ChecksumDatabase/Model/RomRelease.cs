// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Model;

public sealed record class RomRelease
{
    public RomRelease(RomGame parentGame)
    {
        Requires.NotNull(parentGame);

        this.ParentGame = parentGame;
        this.Name = string.Empty;
        this.Region = string.Empty;
        this.Language = string.Empty;
        this.Date = string.Empty;
    }

    public RomGame ParentGame { get; }

    public string Name { get; set; }

    public string Region { get; set; }

    public string Language { get; set; }

    public string Date { get; set; }

    public bool IsDefault { get; set; }
}
