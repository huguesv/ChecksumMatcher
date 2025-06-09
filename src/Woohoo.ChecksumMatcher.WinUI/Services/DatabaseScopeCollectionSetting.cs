// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

public record class DatabaseScopeCollectionSetting<T> where T : class
{
    public Dictionary<string, T[]> Entries { get; set; } = [];
}
