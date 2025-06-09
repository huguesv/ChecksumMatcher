// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using System;
using System.Globalization;
using System.Text;

public static class ChecksumConversion
{
    public static string ToHex(byte[] data)
    {
        Requires.NotNull(data);

#if NET9_0_OR_GREATER
        return Convert.ToHexStringLower(data);
#else
        var text = new StringBuilder();
        foreach (var b in data)
        {
            _ = text.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        }

        return text.ToString();
#endif
    }

    public static byte[] ToByteArray(string hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            return [];
        }

        if (hex.StartsWith("0X", StringComparison.OrdinalIgnoreCase))
        {
            hex = hex[2..];
        }

        if (hex.Length % 2 != 0)
        {
            hex = "0" + hex; // Pad with leading zero if odd length
        }

#if NET9_0_OR_GREATER
        return Convert.FromHexString(hex);
#else
        var data = new byte[hex.Length / 2];
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = (byte)((HexToByte(hex[i * 2]) << 4) | HexToByte(hex[(i * 2) + 1]));
        }

        return data;

        static byte HexToByte(char hex)
        {
            hex = char.ToLowerInvariant(hex);

            if (hex is < '0' or > 'f')
            {
                throw new ArgumentOutOfRangeException(nameof(hex));
            }

            return hex is >= '0' and <= '9' ? (byte)(hex - '0') : (byte)(hex - 'a' + 0x0a);
        }
#endif
    }
}
