// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

class DispatcherQueueService : IDispatcherQueueService
{
    public Windows.System.DispatcherQueue GetWindowsDispatcher()
    {
        return Windows.System.DispatcherQueue.GetForCurrentThread();
    }

    public Microsoft.UI.Dispatching.DispatcherQueue GetUIDispatcher()
    {
        return Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    }
}
