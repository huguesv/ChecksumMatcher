// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using System.Collections.ObjectModel;

public partial class Storage
{
    public Storage()
    {
        this.FolderPaths = [];
        this.OfflineFolders = [];
    }

    public Storage(string folder)
        : this()
    {
        this.FolderPaths.Add(folder);
    }

    public Storage(params string[] folders)
        : this()
    {
        foreach (var folder in folders)
        {
            this.FolderPaths.Add(folder);
        }
    }

    public Collection<string> FolderPaths { get; }

    public Collection<OfflineStorage> OfflineFolders { get; }
}
