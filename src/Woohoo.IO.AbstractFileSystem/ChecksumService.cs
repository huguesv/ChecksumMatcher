// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

using System;
using System.IO;
using Woohoo.Security.Cryptography;

public static class ChecksumService
{
    public static void CalculateAll(FileInformation file, Stream stream, long length)
    {
        Requires.NotNull(file);
        Requires.NotNull(stream);

        var result = new HashCalculator().Calculate(new string[] { "CRC32", "MD5", "SHA1", "SHA256" }, stream, length);
        file.CRC32 = result.Checksums["CRC32"];
        file.MD5 = result.Checksums["MD5"];
        file.SHA1 = result.Checksums["SHA1"];
        file.SHA256 = result.Checksums["SHA256"];
    }
}
