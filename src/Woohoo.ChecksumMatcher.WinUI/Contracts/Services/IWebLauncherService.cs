// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

using System;
using System.Threading.Tasks;

public interface IWebLauncherService
{
    Task LaunchUriAsync(Uri uri);
}
