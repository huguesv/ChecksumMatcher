// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Model;

using System;

public class RomHeaderDataTest : RomHeaderTest
{
    private byte[] values;

    public RomHeaderDataTest()
    {
        this.values = Array.Empty<byte>();
    }

    public long Offset { get; set; }

    public void SetValues(byte[] data)
    {
        Requires.NotNull(data);

        this.values = new byte[data.Length];
        Array.Copy(data, this.values, data.Length);
    }

    public byte[] GetValues()
    {
        var data = new byte[this.values.Length];
        Array.Copy(this.values, data, this.values.Length);

        return data;
    }
}
