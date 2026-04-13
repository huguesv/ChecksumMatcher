// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using System.Collections.Immutable;

public interface IRedumpArtifactsService
{
    void DeleteContents(string folderPath);

    ImmutableArray<string> CleanupContents(string folderPath);
}
