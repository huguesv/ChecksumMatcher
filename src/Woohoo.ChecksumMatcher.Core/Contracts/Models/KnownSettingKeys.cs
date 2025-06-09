// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public static class KnownSettingKeys
{
    // Global settings
    public const string OfflineFolders = nameof(OfflineFolders);
    public const string DatabaseFolders = nameof(DatabaseFolders);
    public const string CueFolders = nameof(CueFolders);
    public const string AppBackgroundRequestedTheme = nameof(AppBackgroundRequestedTheme);
    public const string ExplorerFolderSortPriority = nameof(ExplorerFolderSortPriority);
    public const string ExplorerSortOrder = nameof(ExplorerSortOrder);
    public const string ExplorerArchiveGrouping = nameof(ExplorerArchiveGrouping);
    public const string ExplorerShowArchiveInTree = nameof(ExplorerShowArchiveInTree);
    public const string RedumpUser = nameof(RedumpUser);
    public const string RedumpPassword = nameof(RedumpPassword);
    public const string RedumpDownloadFolder = nameof(RedumpDownloadFolder);
    public const string UseWindowsNotifications = nameof(UseWindowsNotifications);
    public const string UseSystemSounds = nameof(UseSystemSounds);

    // Database file/folder settings
    public const string DatabaseFileScanSettings = nameof(DatabaseFileScanSettings);
    public const string DatabaseFolderScanSettings = nameof(DatabaseFolderScanSettings);
}
