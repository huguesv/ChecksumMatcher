// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Model;

using System.Collections.ObjectModel;

public class RomHeaderRule
{
    public Collection<RomHeaderTest> Tests { get; } = new Collection<RomHeaderTest>();

    public long StartOffset { get; set; }

    public long EndOffset { get; set; } = long.MaxValue;

    public RomHeaderRuleOperation Operation { get; set; }
}
