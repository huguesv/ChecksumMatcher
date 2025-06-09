// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public static class KnownSettingKeys
{
    // Global settings
    public static readonly string OfflineFolders = ToCamelCase(nameof(OfflineFolders));
    public static readonly string DatabaseFolders = ToCamelCase(nameof(DatabaseFolders));
    public static readonly string CueFolders = ToCamelCase(nameof(CueFolders));
    public static readonly string AppBackgroundRequestedTheme = ToCamelCase(nameof(AppBackgroundRequestedTheme));
    public static readonly string ExplorerFolderSortPriority = ToCamelCase(nameof(ExplorerFolderSortPriority));
    public static readonly string ExplorerSortOrder = ToCamelCase(nameof(ExplorerSortOrder));
    public static readonly string ExplorerArchiveGrouping = ToCamelCase(nameof(ExplorerArchiveGrouping));
    public static readonly string ExplorerShowArchiveInTree = ToCamelCase(nameof(ExplorerShowArchiveInTree));
    public static readonly string RedumpUser = ToCamelCase(nameof(RedumpUser));
    public static readonly string RedumpPassword = ToCamelCase(nameof(RedumpPassword));
    public static readonly string RedumpSystems = ToCamelCase(nameof(RedumpSystems));
    public static readonly string RedumpDownloadFolder = ToCamelCase(nameof(RedumpDownloadFolder));
    public static readonly string UseWindowsNotifications = ToCamelCase(nameof(UseWindowsNotifications));
    public static readonly string UseSystemSounds = ToCamelCase(nameof(UseSystemSounds));
    public static readonly string UseWinUIFolderPicker = ToCamelCase(nameof(UseWinUIFolderPicker));
    public static readonly string Win32FolderPickerLastLocations = ToCamelCase(nameof(Win32FolderPickerLastLocations));

    // Database file/folder settings
    public static readonly string RebuildSettings = ToCamelCase(nameof(RebuildSettings));
    public static readonly string DatabaseFileScanSettings = ToCamelCase(nameof(DatabaseFileScanSettings));
    public static readonly string DatabaseFolderScanSettings = ToCamelCase(nameof(DatabaseFolderScanSettings));

    private static string ToCamelCase(string text) => char.ToLowerInvariant(text[0]) + text[1..];
}
