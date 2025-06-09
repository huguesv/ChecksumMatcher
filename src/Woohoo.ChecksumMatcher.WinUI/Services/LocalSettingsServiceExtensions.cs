// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using System.Collections.Generic;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;

internal static class LocalSettingsServiceExtensions
{
    public static T LoadDatabaseScopeSetting<T>(this ILocalSettingsService localSettingsService, string databaseName, string settingKey, T defaultValue)
    {
        var settingValue = localSettingsService.ReadSetting<DatabaseScopeSetting<T>>(settingKey);
        if (settingValue is not null && settingValue.Entries.TryGetValue(databaseName, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    public static void SaveDatabaseScopeSetting<T>(this ILocalSettingsService localSettingsService, string databaseName, string settingKey, T defaultValue)
    {
        var settingValue = localSettingsService.ReadSetting<DatabaseScopeSetting<T>>(settingKey);
        if (settingValue is null)
        {
            settingValue = new DatabaseScopeSetting<T>();
        }

        settingValue.Entries[databaseName] = defaultValue;

        localSettingsService.SaveSetting(settingKey, settingValue);
    }

    public static IEnumerable<T> LoadDatabaseScopeCollectionSetting<T>(this ILocalSettingsService localSettingsService, string databaseName, string settingKey) where T : class
    {
        var settingValue = localSettingsService.ReadSetting<DatabaseScopeCollectionSetting<T>>(settingKey);
        if (settingValue is not null && settingValue.Entries.TryGetValue(databaseName, out var folderArray) && folderArray is not null)
        {
            return folderArray;
        }

        return [];
    }

    public static void SaveDatabaseScopeCollectionSetting<T>(this ILocalSettingsService localSettingsService, string databaseName, string settingKey, T[] value) where T : class
    {
        var settingValue = localSettingsService.ReadSetting<DatabaseScopeCollectionSetting<T>>(settingKey);
        if (settingValue is null)
        {
            settingValue = new DatabaseScopeCollectionSetting<T>();
        }

        settingValue.Entries[databaseName] = value;

        localSettingsService.SaveSetting(settingKey, settingValue);
    }
}
