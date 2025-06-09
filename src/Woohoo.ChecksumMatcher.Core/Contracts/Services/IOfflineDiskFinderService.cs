// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using System.Collections.Generic;

public interface IOfflineDiskFinderService
{
    IEnumerable<OfflineDiskFindResult> FindOfflineDisks();

    OfflineDiskFindResult? TryLoadByName(string diskName);

    OfflineDiskFindResult Load(string filePath);
}
