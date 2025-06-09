// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.UnitTest.Infrastructure;

using System;
using Woohoo.Security.Cryptography;

internal class RandomFile
{
    public RandomFile(string filePath, long size, int seed)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("Invalid file path"));

        var random = new Random(seed);
        var data = new byte[size];
        random.NextBytes(data);
        File.WriteAllBytes(filePath, data);

        var calc = new HashCalculator();
        this.Checksums = calc.Calculate(["crc32", "md5", "sha1", "sha256"], filePath).Checksums;
        this.FilePath = filePath;
        this.Size = size;
    }

    public SortedDictionary<string, byte[]> Checksums { get; }

    public string FilePath { get; }

    public long Size { get; }

    public string FileName => Path.GetFileName(this.FilePath);

    public string FolderName => Path.GetFileName(this.FolderPath);

    public string FolderPath => Path.GetDirectoryName(this.FilePath) ?? throw new NotSupportedException($"Invalid path: {this.FilePath}");
}
