// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public sealed class DatabaseFile
{
    public required string RootAbsoluteFolderPath { get; init; }

    public required string RelativePath { get; init; }

    public string FullPath => Path.Combine(this.RootAbsoluteFolderPath, this.RelativePath);
}
