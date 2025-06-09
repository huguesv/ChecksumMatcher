// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;

using System.Threading.Tasks;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Models;

internal class TestFilePickerService : IFilePickerService
{
    private readonly string[] paths;
    private int currentIndex = 0;

    public TestFilePickerService(params IEnumerable<string> paths)
    {
        this.paths = [.. paths];
    }

    public Task<string?> GetOpenFilePathAsync(string settingsIdentifier, params (string Name, string ExtensionList)[] filters)
    {
        return this.GetNextPath();
    }

    public Task<string?> GetOpenFolderPathAsync(string settingsIdentifier)
    {
        return this.GetNextPath();
    }

    public Task<string?> GetSaveFilePathAsync(string settingsIdentifier, params (string Name, string ExtensionList)[] filters)
    {
        return this.GetNextPath();
    }

    public Task<OfflineDiskFolder?> GetOfflineDiskFolderAsync()
    {
        throw new NotImplementedException();
    }

    private Task<string?> GetNextPath()
    {
        if (this.currentIndex < this.paths.Length)
        {
            var path = this.paths[this.currentIndex];
            this.currentIndex++;
            return Task.FromResult<string?>(path);
        }
        else
        {
            return Task.FromResult<string?>(null);
        }
    }
}
