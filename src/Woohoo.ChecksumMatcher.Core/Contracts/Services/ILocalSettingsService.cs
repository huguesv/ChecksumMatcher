// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

public interface ILocalSettingsService
{
    T? ReadSetting<T>(string key);

    void SaveSetting<T>(string key, T value);

    event EventHandler<SettingChangedEventArgs>? SettingChanged;
}
