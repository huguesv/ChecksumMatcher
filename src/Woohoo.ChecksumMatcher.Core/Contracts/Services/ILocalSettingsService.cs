// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using Woohoo.ChecksumMatcher.Core.Contracts.Models;

public interface ILocalSettingsService
{
    event EventHandler<SettingChangedEventArgs>? SettingChanged;

    T? ReadSetting<T>(string key);

    void SaveSetting<T>(string key, T value);
}
