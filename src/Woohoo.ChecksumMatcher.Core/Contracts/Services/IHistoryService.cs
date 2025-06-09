// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using System.Collections.Immutable;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;

public interface IHistoryService
{
    event EventHandler? ItemsReset;

    event EventHandler<HistoryItemEventArgs>? ItemAdded;

    event EventHandler<HistoryItemEventArgs>? ItemUpdated;

    ImmutableArray<HistoryItem> GetHistoryItems();

    void AddHistoryItem(HistoryItem item);

    bool UpdateStatus(string id, HistoryItemStatus status, string details, DateTimeOffset currentTime);

    void ClearHistory();
}
