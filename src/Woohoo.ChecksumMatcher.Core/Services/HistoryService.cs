// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Services;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;

public sealed class HistoryService : IHistoryService
{
    private readonly Lock itemsLock = new();
    private readonly List<HistoryItem> items = [];

    public event EventHandler? ItemsReset;

    public event EventHandler<HistoryItemEventArgs>? ItemAdded;

    public event EventHandler<HistoryItemEventArgs>? ItemUpdated;

    public void AddHistoryItem(HistoryItem item)
    {
        lock (this.itemsLock)
        {
            this.items.Add(item);
        }

        this.ItemAdded?.Invoke(this, new HistoryItemEventArgs(item));
    }

    public void ClearHistory()
    {
        lock (this.itemsLock)
        {
            this.items.Clear();
        }

        this.ItemsReset?.Invoke(this, EventArgs.Empty);
    }

    public ImmutableArray<HistoryItem> GetHistoryItems()
    {
        lock (this.itemsLock)
        {
            return [.. this.items];
        }
    }

    public bool UpdateStatus(string id, HistoryItemStatus status, string details, DateTimeOffset currentTime)
    {
        HistoryItem? item = null;
        lock (this.itemsLock)
        {
            var index = this.items.FindIndex(i => i.Id == id);
            if (index >= 0)
            {
                item = this.items[index] with { Status = status, Details = details, EndTime = currentTime };
                this.items[index] = item;
            }
        }

        if (item is not null)
        {
            // Send the event outside the lock to avoid potential deadlocks.
            this.ItemUpdated?.Invoke(this, new HistoryItemEventArgs(item));
            return true;
        }

        return false;
    }
}
