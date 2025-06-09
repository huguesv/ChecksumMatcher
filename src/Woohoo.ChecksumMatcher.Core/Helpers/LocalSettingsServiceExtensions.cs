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
    private const string EncryptionKey = "C7A1F9B2E4D6G8H0"; // 16 chars = 128 bits

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
        var settingValue = localSettingsService.ReadSetting<DatabaseScopeSetting<T>>(settingKey);
        if (settingValue is null)
        {
            settingValue = new DatabaseScopeSetting<T>();
        }

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
        var settingValue = localSettingsService.ReadSetting<DatabaseScopeCollectionSetting<T>>(settingKey);
        if (settingValue is null)
        {
            settingValue = new DatabaseScopeCollectionSetting<T>();
        }

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
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
        aes.IV = new byte[16];

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return Convert.ToBase64String(encryptedBytes);
    }

    private static string DecryptString(string encryptedText)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
        aes.IV = new byte[16];

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
