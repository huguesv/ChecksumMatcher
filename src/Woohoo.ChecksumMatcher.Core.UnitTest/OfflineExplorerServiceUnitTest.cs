// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.UnitTest;

using System;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Services;
using Woohoo.ChecksumMatcher.Core.UnitTest.Infrastructure;

public class OfflineExplorerServiceUnitTest
{
    [Fact]
    public async Task GetFolders_NotSet()
    {
        // Arrange
        var localSettingsService = new TestLocalSettingsService();
        var sut = new OfflineExplorerService(new TestLogger<OfflineExplorerService>(), localSettingsService);

        // Act
        var actual = await sut.GetFoldersAsync(CancellationToken.None);

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFolders_Empty()
    {
        // Arrange
        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = { [KnownSettingKeys.OfflineFolders] = Array.Empty<string>() },
        };
        var sut = new OfflineExplorerService(new TestLogger<OfflineExplorerService>(), localSettingsService);

        // Act
        var actual = await sut.GetFoldersAsync(CancellationToken.None);

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOfflineRepository_FoldersNotSet()
    {
        // Arrange
        var localSettingsService = new TestLocalSettingsService();
        var sut = new OfflineExplorerService(new TestLogger<OfflineExplorerService>(), localSettingsService);

        // Act
        var actual = await sut.GetOfflineRepositoryAsync(CancellationToken.None);

        // Assert
        actual.Disks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFolders_FoldersEmpty()
    {
        // Arrange
        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = { [KnownSettingKeys.OfflineFolders] = Array.Empty<string>() },
        };
        var sut = new OfflineExplorerService(new TestLogger<OfflineExplorerService>(), localSettingsService);

        // Act
        var actual = await sut.GetOfflineRepositoryAsync(CancellationToken.None);

        // Assert
        actual.Disks.Should().BeEmpty();
    }
}
