// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.UnitTest.Infrastructure;

using Woohoo.IO.AbstractFileSystem.Offline.Models;

internal class OfflineConfigurationBuilder
{
    public OfflineConfiguration Build()
    {
        return new OfflineConfiguration(new OfflineDiskBuilder().Build());
    }
}
