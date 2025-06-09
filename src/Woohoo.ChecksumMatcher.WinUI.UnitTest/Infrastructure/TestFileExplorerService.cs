// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;

using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

internal class TestFileExplorerService : IFileExplorerService
{
    public string CurrentFilePath { get; set; } = string.Empty;

    public void OpenInExplorer(string filePath)
    {
        this.CurrentFilePath = filePath;
    }
}
