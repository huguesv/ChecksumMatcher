// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.UnitTest;

using Woohoo.IO.AbstractFileSystem.Offline;
using Woohoo.IO.AbstractFileSystem.UnitTest.Infrastructure;

public class OfflineDirectoryUnitTest
{
    [Fact]
    public void EnumerateDirectories()
    {
        // Arrange
        var directory = new OfflineDirectory(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = directory.EnumerateDirectories(@"C:\Data");

        // Assert
        actual.Should().BeEquivalentTo(
        [
            @"C:\Data\SubFolder",
        ]);
    }

    [Fact]
    public void EnumerateFiles()
    {
        // Arrange
        var directory = new OfflineDirectory(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = directory.EnumerateFiles(@"C:\Data");

        // Assert
        actual.Should().BeEquivalentTo(
        [
            @"C:\Data\File1.txt",
            @"C:\Data\File2.bin",
        ]);
    }

    [Fact]
    public void EnumerateFiles_Pattern()
    {
        // Arrange
        var directory = new OfflineDirectory(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = directory.EnumerateFiles(@"C:\Data", "*.bin");

        // Assert
        actual.Should().BeEquivalentTo(
        [
            @"C:\Data\File2.bin",
        ]);
    }

    [Fact]
    public void Exists()
    {
        // Arrange
        var directory = new OfflineDirectory(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = directory.Exists(@"C:\Data\SubFolder");

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void GetCreationTime()
    {
        // Arrange
        var directory = new OfflineDirectory(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = directory.GetCreationTime(@"C:\Data\SubFolder");

        // Assert
        actual.Should().Be(new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc).ToLocalTime());
    }

    [Fact]
    public void GetCreationTime_NotFound()
    {
        // Arrange
        var directory = new OfflineDirectory(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = directory.GetCreationTime(@"C:\Data\SubFolderNotExist");

        // Assert
        actual.Should().Be(new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime());
    }

    [Fact]
    public void GetCreationTimeUtc()
    {
        // Arrange
        var directory = new OfflineDirectory(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = directory.GetCreationTimeUtc(@"C:\Data\SubFolder");

        // Assert
        actual.Should().Be(new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void GetCreationTimeUtc_NotFound()
    {
        // Arrange
        var directory = new OfflineDirectory(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = directory.GetCreationTimeUtc(@"C:\Data\SubFolderNotExist");

        // Assert
        actual.Should().Be(new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void GetLastWriteTime()
    {
        // Arrange
        var directory = new OfflineDirectory(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = directory.GetCreationTime(@"C:\Data\SubFolder");

        // Assert
        actual.Should().Be(new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc).ToLocalTime());
    }
}
