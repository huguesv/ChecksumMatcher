// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;

using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

internal class TestClipboardService : IClipboardService
{
    public string CurrentText { get; set; } = string.Empty;

    public void SetText(string text)
    {
        this.CurrentText = text;
    }
}
