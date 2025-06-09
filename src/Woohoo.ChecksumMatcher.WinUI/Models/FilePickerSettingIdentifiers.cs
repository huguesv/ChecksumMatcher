// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Models;

internal static class FilePickerSettingIdentifiers
{
    public static readonly string CreateDatabaseSourceFolder = ToCamelCase(nameof(CreateDatabaseSourceFolder));
    public static readonly string CreateDatabaseTargetFile = ToCamelCase(nameof(CreateDatabaseTargetFile));
    public static readonly string ScanOnlineFolder = ToCamelCase(nameof(ScanOnlineFolder));
    public static readonly string RebuildSourceFolder = ToCamelCase(nameof(RebuildSourceFolder));
    public static readonly string RebuildTargetFolder = ToCamelCase(nameof(RebuildTargetFolder));
    public static readonly string RebuildTargetIncompleteFolder = ToCamelCase(nameof(RebuildTargetIncompleteFolder));
    public static readonly string HashFile = ToCamelCase(nameof(HashFile));
    public static readonly string OfflineStorageTargetFolder = ToCamelCase(nameof(OfflineStorageTargetFolder));
    public static readonly string RedumpDownloadFolder = ToCamelCase(nameof(RedumpDownloadFolder));
    public static readonly string DatabaseFolder = ToCamelCase(nameof(DatabaseFolder));
    public static readonly string CueFolder = ToCamelCase(nameof(CueFolder));
    public static readonly string OfflineStorageFolder = ToCamelCase(nameof(OfflineStorageFolder));

    private static string ToCamelCase(string text) => char.ToLowerInvariant(text[0]) + text[1..];
}
