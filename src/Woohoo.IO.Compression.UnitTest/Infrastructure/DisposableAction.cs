// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.UnitTest.Infrastructure;

using System;

internal class DisposableAction : IDisposable
{
    private readonly Action action;

    public DisposableAction(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        this.action = action;
    }

    public void Dispose()
    {
        this.action?.Invoke();
    }
}
