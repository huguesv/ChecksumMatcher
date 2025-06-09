// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Model;

using System.Collections.ObjectModel;

public record class RomHeader
{
    public RomHeader()
    {
        this.Name = string.Empty;
        this.Author = string.Empty;
        this.Version = string.Empty;
        this.Rules = new Collection<RomHeaderRule>();
    }

    public string Name { get; set; }

    public string Author { get; set; }

    public string Version { get; set; }

    public Collection<RomHeaderRule> Rules { get; }
}
