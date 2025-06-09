// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.UnitTest;

using System.IO;
using Woohoo.IO.Compression.SevenZip;
using Woohoo.IO.Compression.UnitTest.Infrastructure;

public class SevenZipFileUnitTest
{
    [Fact]
    public void Open()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "This test is only applicable on Windows.");

        // Arrange
        using var tempArchiveFile = DisposableTempFile.WithBytes(Archives.sevenzip_multi_data);

        // Act
        var archive = new SevenZipFile(tempArchiveFile.FilePath);

        // Assert
        archive.Entries.Should().HaveCount(3);

        var entry = archive.Entries[2];
        entry.Name.Should().Be("multi3.bin");
        entry.Size.Should().Be(64u);
        entry.CRC32.Should().Be(0xe23eff1b);
    }

    [Fact]
    public void Extract()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "This test is only applicable on Windows.");

        // Arrange
        using var tempEntryFile = new DisposableTempFile();
        using var tempArchiveFile = DisposableTempFile.WithBytes(Archives.sevenzip_multi_data);
        var archive = new SevenZipFile(tempArchiveFile.FilePath);

        // Act
        archive.Extract(archive.Entries[2], tempEntryFile.FilePath);

        // Assert
        new FileInfo(tempEntryFile.FilePath).Length.Should().Be(64);
    }
}
