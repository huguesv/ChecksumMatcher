// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Model;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public record class RomGame
{
    public RomGame(RomDatabase parentDatabase)
    {
        Requires.NotNull(parentDatabase);

        this.ParentDatabase = parentDatabase;
        this.Name = string.Empty;
        this.Comments = [];
        this.Description = string.Empty;
        this.Year = string.Empty;
        this.Manufacturer = string.Empty;
        this.SourceFile = string.Empty;
        this.RomOf = string.Empty;
        this.CloneOf = string.Empty;
        this.SampleOf = string.Empty;
        this.Board = string.Empty;
        this.RebuildTo = string.Empty;
        this.Releases = [];
        this.BiosSets = [];
        this.Roms = [];
        this.Disks = [];
        this.Samples = [];
        this.Archives = [];
        this.Details = new SortedDictionary<string, string>(new DetailsComparer());
    }

    public RomDatabase ParentDatabase { get; }

    public string Name { get; set; }

    public Collection<string> Comments { get; }

    public SortedDictionary<string, string> Details { get; }

    public string Description { get; set; }

    public string Year { get; set; }

    public string Manufacturer { get; set; }

    public bool IsBios { get; set; }

    public string SourceFile { get; set; }

    public string RomOf { get; set; }

    public string CloneOf { get; set; }

    public string SampleOf { get; set; }

    public string Board { get; set; }

    public string RebuildTo { get; set; }

    public Collection<RomRelease> Releases { get; }

    public Collection<RomBiosSet> BiosSets { get; }

    public Collection<RomFile> Roms { get; }

    public Collection<RomDisk> Disks { get; }

    public Collection<RomSample> Samples { get; }

    public Collection<RomArchive> Archives { get; }

    public static int CompareByName(RomGame x, RomGame y)
    {
        Requires.NotNull(x);
        Requires.NotNull(y);

        return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }

    internal class NameComparer : IComparer<RomGame>
    {
        public int Compare(RomGame? x, RomGame? y)
        {
            Requires.NotNull(x);
            Requires.NotNull(y);

            return RomGame.CompareByName(x!, y!);
        }
    }
}
