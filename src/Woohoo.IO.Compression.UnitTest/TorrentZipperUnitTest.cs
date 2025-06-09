// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.UnitTest;

using System;
using Woohoo.IO.Compression.TorrentZip;
using Woohoo.IO.Compression.UnitTest.Infrastructure;
using Woohoo.IO.Compression.UnitTest.Infrastructure.Assertions;

public class TorrentZipperUnitTest
{
    [Fact]
    public async Task RezipExisting()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "This test is only applicable on Windows.");

        // Arrange
        using var tempArchiveFile = DisposableTempFile.WithBytes(Archives.zip_multi_data);
        var archiveFilePath = Path.ChangeExtension(tempArchiveFile.FilePath, ".zip");
        File.Move(tempArchiveFile.FilePath, archiveFilePath);

        // Act
        await TorrentZipper.TorrentZipAsync(archiveFilePath, CancellationToken.None);

        // Assert
        var archiveFileInfo = new FileInfo(archiveFilePath);
        archiveFileInfo.Exists.Should().BeTrue();
        archiveFileInfo.Should().BeTorrentZip();
    }

    [Fact]
    public async Task RezipInvalidFileExtension()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "This test is only applicable on Windows.");

        // Arrange
        using var tempArchiveFile = DisposableTempFile.WithBytes(Archives.zip_multi_data);
        var archiveFilePath = Path.ChangeExtension(tempArchiveFile.FilePath, ".zop");
        File.Move(tempArchiveFile.FilePath, archiveFilePath);

        // Act
        Func<Task> act = async () => await TorrentZipper.TorrentZipAsync(archiveFilePath, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RezipInvalidFileFormat()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "This test is only applicable on Windows.");

        // Arrange
        using var tempArchiveFile = DisposableTempFile.WithBytes(Archives.multi1);
        var archiveFilePath = Path.ChangeExtension(tempArchiveFile.FilePath, ".zip");
        File.Move(tempArchiveFile.FilePath, archiveFilePath);

        // Act
        Func<Task> act = async () => await TorrentZipper.TorrentZipAsync(archiveFilePath, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<CompressionException>();
    }
}
