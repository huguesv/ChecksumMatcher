// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;

using System;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;

internal class TestDateTimeProviderService : IDateTimeProviderService
{
    public TestDateTimeProviderService()
        : this(DateTime.Now, DateTime.UtcNow)
    {
    }

    public TestDateTimeProviderService(DateTime now, DateTime utcNow)
    {
        this.Now = now;
        this.UtcNow = utcNow;
    }

    public DateTime Now { get; }

    public DateTime UtcNow { get; }
}
