// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.UnitTest;

using Woohoo.IO.AbstractFileSystem.Offline;
using Woohoo.IO.AbstractFileSystem.UnitTest.Infrastructure;

public class OfflineFileInfoUnitTest
{
    [Fact]
    public void Exists()
    {
        // Arrange
        var configuration = new OfflineConfigurationBuilder().Build();
        var fileItem = configuration.GetItemByPath(@"C:\Data\File1.txt");
        var fileInfo = new OfflineFileInfo(configuration, fileItem);

        // Act
        var actual = fileInfo.Exists;

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void CreationTime()
    {
        // Arrange
        var configuration = new OfflineConfigurationBuilder().Build();
        var fileItem = configuration.GetItemByPath(@"C:\Data\File1.txt");
        var fileInfo = new OfflineFileInfo(configuration, fileItem);

        // Act
        var actual = fileInfo.CreationTime;

        // Assert
        actual.Should().Be(new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc).ToLocalTime());
    }

    [Fact]
    public void CreationTime_NotFound()
    {
        // Arrange
        var configuration = new OfflineConfigurationBuilder().Build();
        var fileInfo = new OfflineFileInfo(configuration, @"C:\Data\FileNotExist");

        // Act
        var actual = fileInfo.CreationTime;

        // Assert
        actual.Should().Be(new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime());
    }

    [Fact]
    public void CreationTimeUtc()
    {
        // Arrange
        var configuration = new OfflineConfigurationBuilder().Build();
        var fileItem = configuration.GetItemByPath(@"C:\Data\File1.txt");
        var fileInfo = new OfflineFileInfo(configuration, fileItem);

        // Act
        var actual = fileInfo.CreationTimeUtc;

        // Assert
        actual.Should().Be(new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void CreationTimeUtc_NotFound()
    {
        // Arrange
        var configuration = new OfflineConfigurationBuilder().Build();
        var fileInfo = new OfflineFileInfo(configuration, @"C:\Data\FileNotExist");

        // Act
        var actual = fileInfo.CreationTimeUtc;

        // Assert
        actual.Should().Be(new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}
