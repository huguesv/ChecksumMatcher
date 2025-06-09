// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;

using System;
using System.Threading.Tasks;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

internal class TestWebLauncherService : IWebLauncherService
{
    public Uri? CurrentUri { get; set; } = null;

    public Task LaunchUriAsync(Uri uri)
    {
        this.CurrentUri = uri;
        return Task.CompletedTask;
    }
}
