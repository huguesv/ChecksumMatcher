// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using System;
using System.Threading.Tasks;
using Windows.System;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

internal sealed class WebLauncherService : IWebLauncherService
{
    public async Task LaunchUriAsync(Uri uri)
    {
        await Launcher.LaunchUriAsync(uri);
    }
}
