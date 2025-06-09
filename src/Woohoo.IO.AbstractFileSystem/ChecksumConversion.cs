// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

public static class ChecksumConversion
{
    public static byte[] ToByteArray(uint crc)
    {
        var output = new byte[4];

        output[0] = (byte)(crc >> 24);
        output[1] = (byte)(crc >> 16);
        output[2] = (byte)(crc >> 8);
        output[3] = (byte)crc;

        return output;
    }

    public static string ToHex(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return Convert.ToHexStringLower(data);
    }

    public static byte[] ToByteArray(string? hex)
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

        return Convert.FromHexString(hex);
    }
}
