// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

internal class DispatcherQueueService : IDispatcherQueueService
{
    public IDispatcherQueue GetDispatcherQueue()
    {
        return new WindowsSystemDispatcherQueue(Windows.System.DispatcherQueue.GetForCurrentThread());
    }
}
