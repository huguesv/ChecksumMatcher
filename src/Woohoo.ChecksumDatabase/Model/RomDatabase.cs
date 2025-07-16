// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Model;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public record class RomDatabase
{
    public RomDatabase()
    {
        this.Name = string.Empty;
        this.Description = string.Empty;
        this.Category = string.Empty;
        this.Version = string.Empty;
        this.Date = string.Empty;
        this.Author = string.Empty;
        this.Email = string.Empty;
        this.Homepage = string.Empty;
        this.Url = string.Empty;
        this.Comment = string.Empty;
        this.EmulatorName = string.Empty;
        this.EmulatorVersion = string.Empty;
        this.Games = [];
    }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Category { get; set; }

    public string Version { get; set; }

    public string Date { get; set; }

    public string Author { get; set; }

    public string Email { get; set; }

    public string Homepage { get; set; }

    public string Url { get; set; }

    public string Comment { get; set; }

    public string EmulatorName { get; set; }

    public string EmulatorVersion { get; set; }

    public Collection<RomGame> Games { get; }

    public RomFile[] GetAllRoms()
    {
        return [.. this.Games.SelectMany(game => game.Roms)];
    }

    public void SortGames()
    {
        var sortedGames = this.Games.ToArray();
        Array.Sort(sortedGames, new RomGame.NameComparer());

        this.Games.Clear();

        foreach (var game in sortedGames)
        {
            this.Games.Add(game);
        }
    }
}
