// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using System.Collections.Generic;
using System.Collections.Immutable;
using Woohoo.ChecksumDatabase.Model;

internal sealed class DatabaseIndex
{
    private readonly RomDatabase database;

    private readonly object editLock = new();
    private readonly Dictionary<string, RomGame> nameToGameDict = [];
    private readonly Dictionary<string, List<RomGame>> cloneOfGames = [];
    private readonly Dictionary<string, List<RomGame>> romOfGames = [];

    public DatabaseIndex(RomDatabase db)
    {
        this.database = db;

        foreach (var game in db.Games)
        {
            this.nameToGameDict[game.Name] = game;

            if (!string.IsNullOrEmpty(game.CloneOf))
            {
                if (!this.cloneOfGames.TryGetValue(game.CloneOf, out var list))
                {
                    list = [];
                    this.cloneOfGames[game.CloneOf] = list;
                }

                list.Add(game);
            }

            if (!string.IsNullOrEmpty(game.RomOf))
            {
                if (!this.romOfGames.TryGetValue(game.RomOf, out var list))
                {
                    list = [];
                    this.romOfGames[game.RomOf] = list;
                }

                list.Add(game);
            }
        }
    }

    public ImmutableArray<RomGame> AllGames
    {
        get
        {
            lock (this.editLock)
            {
                return [.. this.database.Games];
            }
        }
    }

    public RomGame? FindGameByName(string name)
    {
        lock (this.editLock)
        {
            return this.nameToGameDict.TryGetValue(name, out var game) ? game : null;
        }
    }

    public ImmutableArray<RomGame> FindClonesOf(string name)
    {
        lock (this.editLock)
        {
            return this.cloneOfGames.TryGetValue(name, out var list) ? [.. list] : [];
        }
    }

    public ImmutableArray<RomGame> FindRomsOf(string name)
    {
        lock (this.editLock)
        {
            return this.romOfGames.TryGetValue(name, out var list) ? [.. list] : [];
        }
    }

    public void RemoveGame(RomGame game)
    {
        lock (this.editLock)
        {
            if (this.nameToGameDict.Remove(game.Name))
            {
                if (!string.IsNullOrEmpty(game.CloneOf) && this.cloneOfGames.TryGetValue(game.CloneOf, out var cloneList))
                {
                    cloneList.Remove(game);
                    if (cloneList.Count == 0)
                    {
                        this.cloneOfGames.Remove(game.CloneOf);
                    }
                }

                if (!string.IsNullOrEmpty(game.RomOf) && this.romOfGames.TryGetValue(game.RomOf, out var romList))
                {
                    romList.Remove(game);
                    if (romList.Count == 0)
                    {
                        this.romOfGames.Remove(game.RomOf);
                    }
                }

                this.database.Games.Remove(game);
            }
        }
    }
}
