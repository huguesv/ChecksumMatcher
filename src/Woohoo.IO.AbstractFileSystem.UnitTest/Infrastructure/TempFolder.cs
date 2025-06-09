// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.UnitTest.Infrastructure;

internal class TempFolder : IDisposable
{
    public TempFolder()
    {
        this.Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.Path);
    }

    public string Path { get; }

    public void Dispose()
    {
        if (Directory.Exists(this.Path))
        {
            Directory.Delete(this.Path, true);
        }
    }
}
