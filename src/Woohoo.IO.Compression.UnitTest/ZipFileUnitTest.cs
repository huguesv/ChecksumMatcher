// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.UnitTest;

using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Woohoo.IO.Compression.UnitTest.Infrastructure;
using Woohoo.IO.Compression.Zip;

public class ZipFileUnitTest
{
    [Fact]
    public void Open()
    {
        // Arrange
        using var tempArchiveFile = DisposableTempFile.WithBytes(Archives.bin_data);

        // Act
        using var archive = new ZipFile(tempArchiveFile.FilePath);

        // Assert
        archive.Count.Should().Be(1);

        var entry = archive[0];
        entry.Name.Should().Be("bin-data.bin");
        entry.Size.Should().Be(10);
        entry.Crc.Should().Be(0x456cd746);
    }

    [Fact]
    public void Extract()
    {
        // Arrange
        using var tempEntryFile = new DisposableTempFile();
        using var tempArchiveFile = DisposableTempFile.WithBytes(Archives.bin_data);
        using var archive = new ZipFile(tempArchiveFile.FilePath);

        // Act
        archive.Extract(archive[0], tempEntryFile.FilePath);

        // Assert
        new FileInfo(tempEntryFile.FilePath).Length.Should().Be(10);
    }

    [Theory]
    [InlineData("multi1.bin")]
    [InlineData("multi2.bin")]
    [InlineData("multi3.bin")]
    public void GetStream(string entryName)
    {
        // Arrange
        using var tempArchiveFile = DisposableTempFile.WithBytes(Archives.zip_multi_data);
        using var archive = new ZipFile(tempArchiveFile.FilePath);
        var entry = archive.GetEntry(entryName);

        // Act
        using var stream = archive.GetStream(entry);
        byte[] buffer = new byte[entry.Size];
        var outputStream = new MemoryStream(buffer);
        stream.CopyTo(outputStream);
        outputStream.Flush();

        // Assert
        var entryNameToExpectedData = new Dictionary<string, byte[]>()
        {
            { "multi1.bin", Archives.multi1 },
            { "multi2.bin", Archives.multi2 },
            { "multi3.bin", Archives.multi3 },
        };
        byte[] expectedData = entryNameToExpectedData[entryName];
        buffer.Should().BeEquivalentTo(expectedData);
    }
}
