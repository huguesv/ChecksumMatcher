// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;

using System;
using System.Threading;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using TimersTimer = System.Timers.Timer;

internal class TestDispatcherQueueService : IDispatcherQueueService
{
    private readonly Lazy<IDispatcherQueue> dispatcherQueue;

    public TestDispatcherQueueService()
    {
        this.dispatcherQueue = new Lazy<IDispatcherQueue>(
            () => new SynchronizationContextQueueService(SynchronizationContext.Current ?? new SynchronizationContext()));
    }

    public IDispatcherQueue GetDispatcherQueue()
    {
        return this.dispatcherQueue.Value;
    }

    private class SynchronizationContextQueueService : IDispatcherQueue
    {
        private readonly SynchronizationContext synchronizationContext;

        public SynchronizationContextQueueService(SynchronizationContext synchronizationContext)
        {
            this.synchronizationContext = synchronizationContext;
        }

        public IDispatcherQueueTimer CreateTimer()
        {
            return new TimerWrapper(new TimersTimer());
        }

        public bool TryEnqueue(Action action)
        {
            this.synchronizationContext.Post(_ => action(), null);
            return true;
        }

        private class TimerWrapper : IDispatcherQueueTimer
        {
            private readonly TimersTimer timer;
            private bool isRepeating;
            private bool isRunning;

            public TimerWrapper(TimersTimer timer)
            {
                this.timer = timer;
                this.timer.AutoReset = true;
                this.timer.Elapsed += (s, e) =>
                {
                    this.Tick?.Invoke(this, EventArgs.Empty);
                    if (!this.IsRepeating)
                    {
                        this.Stop();
                    }
                };
                this.isRepeating = true;
                this.isRunning = false;
            }

            public event EventHandler? Tick;

            public TimeSpan Interval
            {
                get => TimeSpan.FromMilliseconds(this.timer.Interval);
                set => this.timer.Interval = value.TotalMilliseconds;
            }

            public bool IsRepeating
            {
                get => this.isRepeating;
                set
                {
                    this.isRepeating = value;
                    this.timer.AutoReset = value;
                }
            }

            public bool IsRunning => this.isRunning;

            public void Start()
            {
                this.isRunning = true;
                this.timer.Start();
            }

            public void Stop()
            {
                this.isRunning = false;
                this.timer.Stop();
            }
        }
    }
}
