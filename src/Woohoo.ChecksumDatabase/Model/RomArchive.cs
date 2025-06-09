// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Model;

public record class RomArchive
{
    public RomArchive(RomGame parentGame)
    {
        Requires.NotNull(parentGame);

        this.ParentGame = parentGame;
        this.Name = string.Empty;
    }

    public RomGame ParentGame { get; }

    public string Name { get; set; }
}
