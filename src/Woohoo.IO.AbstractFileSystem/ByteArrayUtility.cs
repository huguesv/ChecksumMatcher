// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

public static class ByteArrayUtility
{
    public static bool AreEqual(byte[] a, byte[] b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        if (a.Length != b.Length)
        {
            return false;
        }

        for (var i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }

        return true;
    }

    public static byte[] ByteArrayFromUInt32(uint crc)
    {
        var output = new byte[4];

        output[0] = (byte)(crc >> 24);
        output[1] = (byte)(crc >> 16);
        output[2] = (byte)(crc >> 8);
        output[3] = (byte)crc;

        return output;
    }

    public static byte[] HexToByteArray(string? hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            return [];
        }

        int length = hex.Length / 2;
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return bytes;
    }

    public static string ByteArrayToHex(byte[] data)
    {
        return BitConverter.ToString(data).Replace("-", string.Empty).ToLowerInvariant();
    }
}
