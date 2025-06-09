// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Helpers;

using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;

public static class LocalSettingsServiceExtensions
{
    public static T LoadItemScopeSetting<T>(this ILocalSettingsService localSettingsService, string itemName, string settingKey, T defaultValue)
    {
        var settingValue = localSettingsService.ReadSetting<DatabaseScopeSetting<T>>(settingKey);
        if (settingValue is not null && settingValue.Entries.TryGetValue(itemName, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    public static void SaveItemScopeSetting<T>(this ILocalSettingsService localSettingsService, string itemName, string settingKey, T defaultValue)
    {
        var settingValue = localSettingsService.ReadSetting<DatabaseScopeSetting<T>>(settingKey)
            ?? new DatabaseScopeSetting<T>();

        settingValue.Entries[itemName] = defaultValue;

        localSettingsService.SaveSetting(settingKey, settingValue);
    }

    public static IEnumerable<T> LoadItemScopeCollectionSetting<T>(this ILocalSettingsService localSettingsService, string itemName, string settingKey)
        where T : class
    {
        var settingValue = localSettingsService.ReadSetting<DatabaseScopeCollectionSetting<T>>(settingKey);
        if (settingValue is not null && settingValue.Entries.TryGetValue(itemName, out var folderArray) && folderArray is not null)
        {
            return folderArray;
        }

        return [];
    }

    public static void SaveItemScopeCollectionSetting<T>(this ILocalSettingsService localSettingsService, string itemName, string settingKey, T[] value)
        where T : class
    {
        var settingValue = localSettingsService.ReadSetting<DatabaseScopeCollectionSetting<T>>(settingKey)
            ?? new DatabaseScopeCollectionSetting<T>();

        settingValue.Entries[itemName] = value;

        localSettingsService.SaveSetting(settingKey, settingValue);
    }

    public static string LoadEncryptedSetting(this ILocalSettingsService localSettingsService, string key, string defaultValue)
    {
        var value = localSettingsService.ReadSetting<string>(key);
        if (value is null)
        {
            return defaultValue;
        }

        if (value.Length == 0)
        {
            return string.Empty;
        }

        return DecryptString(value);
    }

    public static void SaveEncryptedSetting(this ILocalSettingsService localSettingsService, string key, string value)
    {
        var encryptedValue = EncryptString(value);
        localSettingsService.SaveSetting(key, encryptedValue);
    }

    private static string EncryptString(string plainText)
    {
        var dataToProtect = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = ProtectedData.Protect(dataToProtect, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encryptedBytes);
    }

    private static string DecryptString(string encryptedText)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
