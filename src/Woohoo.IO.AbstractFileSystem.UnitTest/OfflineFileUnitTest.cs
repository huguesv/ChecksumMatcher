// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.UnitTest;

using Woohoo.IO.AbstractFileSystem.Offline;
using Woohoo.IO.AbstractFileSystem.UnitTest.Infrastructure;

public class OfflineFileUnitTest
{
    [Fact]
    public void Exists()
    {
        // Arrange
        var file = new OfflineFile(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = file.Exists(@"C:\Data\File1.txt");

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void GetCreationTime()
    {
        // Arrange
        var file = new OfflineFile(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = file.GetCreationTime(@"C:\Data\File1.txt");

        // Assert
        actual.Should().Be(new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc).ToLocalTime());
    }

    [Fact]
    public void GetCreationTime_NotFound()
    {
        // Arrange
        var file = new OfflineFile(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = file.GetCreationTime(@"C:\Data\FileNotExist");

        // Assert
        actual.Should().Be(new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime());
    }

    [Fact]
    public void GetCreationTimeUtc()
    {
        // Arrange
        var file = new OfflineFile(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = file.GetCreationTimeUtc(@"C:\Data\File1.txt");

        // Assert
        actual.Should().Be(new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void GetCreationTimeUtc_NotFound()
    {
        // Arrange
        var file = new OfflineFile(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = file.GetCreationTimeUtc(@"C:\Data\FileNotExist");

        // Assert
        actual.Should().Be(new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void GetLastWriteTime()
    {
        // Arrange
        var file = new OfflineFile(new OfflineConfigurationBuilder().Build());

        // Act
        var actual = file.GetCreationTime(@"C:\Data\File1.txt");

        // Assert
        actual.Should().Be(new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc).ToLocalTime());
    }
}
