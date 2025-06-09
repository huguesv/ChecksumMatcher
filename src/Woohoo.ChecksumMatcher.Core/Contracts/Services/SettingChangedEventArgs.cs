// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

public class SettingChangedEventArgs : EventArgs
{
    public SettingChangedEventArgs(string settingKey)
    {
        this.SettingKey = settingKey;
    }

    public string SettingKey { get; }
}
