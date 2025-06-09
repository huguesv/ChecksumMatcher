// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.UnitTest;

using System;
using Woohoo.IO.Compression.TorrentSevenZip;
using Woohoo.IO.Compression.UnitTest.Infrastructure;
using Woohoo.IO.Compression.UnitTest.Infrastructure.Assertions;

public class TorrentSevenZipperUnitTest
{
    [Fact]
    public async Task RezipExisting()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "This test is only applicable on Windows.");

        // TODO: figure out why we get an error from t7z "unable to uncompress file" on GitHub Actions.
        Assert.SkipWhen(
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE")),
            "Skipping on GitHub Actions as it is not supported yet.");

        // Arrange
        using var tempArchiveFile = DisposableTempFile.WithBytes(Archives.zip_multi_data);

        // Act
        await TorrentSevenZipper.TorrentZipAsync(tempArchiveFile.FilePath, CancellationToken.None);

        // Assert
        var archiveFilePath = Path.ChangeExtension(tempArchiveFile.FilePath, ".7z");
        var archiveFileInfo = new FileInfo(archiveFilePath);
        archiveFileInfo.Exists.Should().BeTrue();
        archiveFileInfo.Should().BeTorrentSevenZip();
    }

    [Fact]
    public async Task RezipInvalidFileFormat()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "This test is only applicable on Windows.");

        // Arrange
        using var tempArchiveFile = DisposableTempFile.WithBytes(Archives.multi1);

        // Act
        Func<Task> act = async () => await TorrentSevenZipper.TorrentZipAsync(tempArchiveFile.FilePath, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<CompressionException>();
    }

    [Fact]
    public async Task CreateNew()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "This test is only applicable on Windows.");

        // Arrange
        using var tempFolder = new DisposableTempFolder();
        var archiveFilePath = Path.Combine(tempFolder.Path, "multi.7z");
        var dataFilePath = Path.Combine(tempFolder.Path, "multi1.bin");
        File.WriteAllBytes(dataFilePath, Archives.multi1);

        // Act
        await TorrentSevenZipper.Create7zAsync(archiveFilePath, dataFilePath, CancellationToken.None);

        // Assert
        var archiveFileInfo = new FileInfo(archiveFilePath);
        archiveFileInfo.Exists.Should().BeTrue();
        archiveFileInfo.Should().BeTorrentSevenZip();
    }
}
