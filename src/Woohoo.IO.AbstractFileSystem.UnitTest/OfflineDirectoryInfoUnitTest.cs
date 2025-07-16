// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.UnitTest;

using Woohoo.IO.AbstractFileSystem.Offline;
using Woohoo.IO.AbstractFileSystem.UnitTest.Infrastructure;

public class OfflineDirectoryInfoUnitTest
{
    [Fact]
    public void EnumerateDirectories()
    {
        // Arrange
        var configuration = new OfflineConfigurationBuilder().Build();
        var directoryItem = configuration.GetItemByPath(@"C:\Data") ?? throw new Exception("Item not found.");
        var directoryInfo = new OfflineDirectoryInfo(configuration, directoryItem);

        // Act
        var actual = directoryInfo.EnumerateDirectories().ToArray();

        // Assert
        actual.Length.Should().Be(1);
        actual[0].FullName.Should().Be(@"C:\Data\SubFolder");
    }

    [Fact]
    public void EnumerateFiles()
    {
        // Arrange
        var configuration = new OfflineConfigurationBuilder().Build();
        var directoryItem = configuration.GetItemByPath(@"C:\Data") ?? throw new Exception("Item not found.");
        var directoryInfo = new OfflineDirectoryInfo(configuration, directoryItem);

        // Act
        var actual = directoryInfo.EnumerateFiles().ToArray();

        // Assert
        actual.Length.Should().Be(2);
        actual[0].FullName.Should().Be(@"C:\Data\File1.txt");
        actual[1].FullName.Should().Be(@"C:\Data\File2.bin");
    }

    [Fact]
    public void EnumerateFiles_Pattern()
    {
        // Arrange
        var configuration = new OfflineConfigurationBuilder().Build();
        var directoryItem = configuration.GetItemByPath(@"C:\Data") ?? throw new Exception("Item not found.");
        var directoryInfo = new OfflineDirectoryInfo(configuration, directoryItem);

        // Act
        var actual = directoryInfo.EnumerateFiles("*.bin").ToArray();

        // Assert
        actual.Length.Should().Be(1);
        actual[0].FullName.Should().Be(@"C:\Data\File2.bin");
    }

    [Fact]
    public void Exists()
    {
        // Arrange
        var configuration = new OfflineConfigurationBuilder().Build();
        var directoryItem = configuration.GetItemByPath(@"C:\Data\SubFolder") ?? throw new Exception("Item not found.");
        var directoryInfo = new OfflineDirectoryInfo(configuration, directoryItem);

        // Act
        var actual = directoryInfo.Exists;

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void CreationTime()
    {
        // Arrange
        var configuration = new OfflineConfigurationBuilder().Build();
        var directoryItem = configuration.GetItemByPath(@"C:\Data\SubFolder") ?? throw new Exception("Item not found.");
        var directoryInfo = new OfflineDirectoryInfo(configuration, directoryItem);

        // Act
        var actual = directoryInfo.CreationTime;

        // Assert
        actual.Should().Be(new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc).ToLocalTime());
    }

    [Fact]
    public void CreationTime_NotFound()
    {
        // Arrange
        var configuration = new OfflineConfigurationBuilder().Build();
        var directoryInfo = new OfflineDirectoryInfo(configuration, @"C:\Data\SubFolderNotExist");

        // Act
        var actual = directoryInfo.CreationTime;

        // Assert
        actual.Should().Be(new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime());
    }

    [Fact]
    public void CreationTimeUtc()
    {
        // Arrange
        var configuration = new OfflineConfigurationBuilder().Build();
        var directoryItem = configuration.GetItemByPath(@"C:\Data\SubFolder") ?? throw new Exception("Item not found.");
        var directoryInfo = new OfflineDirectoryInfo(configuration, directoryItem);

        // Act
        var actual = directoryInfo.CreationTimeUtc;

        // Assert
        actual.Should().Be(new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void CreationTimeUtc_NotFound()
    {
        // Arrange
        var configuration = new OfflineConfigurationBuilder().Build();
        var directoryInfo = new OfflineDirectoryInfo(configuration, @"C:\Data\SubFolderNotExist");

        // Act
        var actual = directoryInfo.CreationTimeUtc;

        // Assert
        actual.Should().Be(new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}
