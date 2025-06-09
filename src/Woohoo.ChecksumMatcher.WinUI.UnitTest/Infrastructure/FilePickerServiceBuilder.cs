// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;

using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

internal class FilePickerServiceBuilder
{
    private readonly List<string> paths = [];

    public FilePickerServiceBuilder WithPath(string path)
    {
        this.paths.Add(path);
        return this;
    }

    public IFilePickerService Build()
    {
        return new TestFilePickerService(this.paths);
    }
}
