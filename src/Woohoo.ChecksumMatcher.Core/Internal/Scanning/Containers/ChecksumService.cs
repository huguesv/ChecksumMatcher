// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers;

using System.IO;
using Woohoo.Security.Cryptography;

internal static class ChecksumService
{
    public static async Task CalculateAllAsync(FileInformation file, Stream stream, long length, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(stream);

        var result = await HashCalculator.CalculateAsync(["CRC32", "MD5", "SHA1", "SHA256"], stream, length, ct);
        file.CRC32 = result.Checksums["CRC32"];
        file.MD5 = result.Checksums["MD5"];
        file.SHA1 = result.Checksums["SHA1"];
        file.SHA256 = result.Checksums["SHA256"];
    }
}
