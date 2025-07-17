// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal;

using System.Collections.Generic;

internal class EffectiveScanSettings
{
    public bool ForceCalculateChecksums { get; set; }

    public List<EffectiveOnlineFolderSetting> ScanOnlineFolders { get; init; } = [];

    public List<EffectiveOfflineFolderSetting> ScanOfflineFolders { get; init; } = [];
}
