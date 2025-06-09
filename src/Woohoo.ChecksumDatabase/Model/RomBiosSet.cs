// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Model;

public sealed record class RomBiosSet
{
    public RomBiosSet(RomGame parentGame)
    {
        Requires.NotNull(parentGame);

        this.ParentGame = parentGame;
        this.Name = string.Empty;
        this.Description = string.Empty;
    }

    public RomGame ParentGame { get; }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsDefault { get; set; }
}
