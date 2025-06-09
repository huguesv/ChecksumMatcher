// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using System;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

internal class WindowsSystemDispatcherQueue : IDispatcherQueue
{
    private readonly Windows.System.DispatcherQueue dispatcherQueue;

    public WindowsSystemDispatcherQueue(Windows.System.DispatcherQueue dispatcherQueue)
    {
        this.dispatcherQueue = dispatcherQueue;
    }

    public IDispatcherQueueTimer CreateTimer()
    {
        return new WindowsSystemDispatcherQueueTimer(this.dispatcherQueue.CreateTimer());
    }

    public bool TryEnqueue(Action action)
    {
        return this.dispatcherQueue.TryEnqueue(() => action());
    }
}
