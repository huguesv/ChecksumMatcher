// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Model;

public class RomHeaderFileTest : RomHeaderTest
{
    public RomHeaderFileTestOperation Operation { get; set; }

    public RomHeaderFileSizeMode SizeMode { get; set; }

    public long Size { get; set; }
}
