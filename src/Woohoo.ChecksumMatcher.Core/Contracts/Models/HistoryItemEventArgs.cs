// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public sealed class HistoryItemEventArgs : EventArgs
{
    public HistoryItemEventArgs(HistoryItem item)
    {
        this.Item = item;
    }

    public HistoryItem Item { get; init; }
}
