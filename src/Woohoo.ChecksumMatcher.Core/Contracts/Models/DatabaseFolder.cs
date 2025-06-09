// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

using System.Collections.Immutable;

public class DatabaseFolder
{
    public required string RootAbsoluteFolderPath { get; init; }

    public required string RelativePath { get; init; }

    public required ImmutableArray<DatabaseFolder> SubFolders { get; init; }

    public required ImmutableArray<DatabaseFile> Files { get; init; }

    public bool IsRoot => string.IsNullOrEmpty(this.RelativePath);

    public string Name => Path.GetFileName(this.IsRoot ? this.RootAbsoluteFolderPath : this.RelativePath);

    public string FullPath => Path.Combine(this.RootAbsoluteFolderPath, this.RelativePath);
}
