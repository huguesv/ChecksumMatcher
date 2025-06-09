// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.UnitTest.Infrastructure;

using System;
using System.Collections.Generic;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;

internal class TestLocalSettingsService : ILocalSettingsService
{
    public event EventHandler<SettingChangedEventArgs>? SettingChanged;

    public Dictionary<string, object?> Settings { get; init; } = [];

    public T? ReadSetting<T>(string key)
    {
        return this.Settings.TryGetValue(key, out var value) ? (T?)value : default;
    }

    public void SaveSetting<T>(string key, T value)
    {
        this.Settings[key] = value;
        this.SettingChanged?.Invoke(this, new SettingChangedEventArgs(key));
    }
}
