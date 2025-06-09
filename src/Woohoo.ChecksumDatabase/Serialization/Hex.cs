// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using System;
using System.Globalization;
using System.Text;

public static class Hex
{
    public static string ByteArrayToText(byte[] data)
    {
        Requires.NotNull(data);

        var text = new StringBuilder();

        foreach (var b in data)
        {
            _ = text.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        }

        return text.ToString();
    }

    public static byte[] TextToByteArray(string text)
    {
        Requires.NotNull(text);

        if (text.StartsWith("0X", StringComparison.OrdinalIgnoreCase))
        {
            text = text[2..];
        }

        if (text.Length % 2 != 0)
        {
            text = "0" + text; // Pad with leading zero if odd length
        }

        var data = new byte[text.Length / 2];

        for (var i = 0; i < data.Length; i++)
        {
            data[i] = (byte)((HexToByte(text[i * 2]) << 4) | HexToByte(text[(i * 2) + 1]));
        }

        return data;
    }

    private static byte HexToByte(char hex)
    {
        hex = char.ToLowerInvariant(hex);

        if (hex is < '0' or > 'f')
        {
            throw new ArgumentOutOfRangeException("hex");
        }

        return hex is >= '0' and <= '9' ? (byte)(hex - '0') : (byte)(hex - 'a' + 0x0a);
    }
}
