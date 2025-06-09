// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Model;

using System;

public class RomHeaderBooleanTest : RomHeaderTest
{
    private byte[] masks;

    private byte[] values;

    public RomHeaderBooleanTest()
    {
        this.masks = new byte[0];
        this.values = new byte[0];
    }

    public RomHeaderBooleanTestOperation Operation { get; set; }

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

    public void SetMasks(byte[] data)
    {
        Requires.NotNull(data);

        this.masks = new byte[data.Length];
        Array.Copy(data, this.masks, data.Length);
    }

    public byte[] GetMasks()
    {
        var data = new byte[this.masks.Length];
        Array.Copy(this.masks, data, this.masks.Length);

        return data;
    }
}
