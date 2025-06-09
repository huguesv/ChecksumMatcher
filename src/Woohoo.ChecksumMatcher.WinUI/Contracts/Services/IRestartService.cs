// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

using Woohoo.ChecksumMatcher.WinUI.Models;

public interface IRestartService
{
    Task RestartAsync();
}
