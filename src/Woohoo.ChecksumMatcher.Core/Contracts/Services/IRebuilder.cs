// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using System;
using Woohoo.ChecksumDatabase.Model;

public interface IRebuilder
{
    event EventHandler<RebuilderProgressEventArgs> Progress;

    void Cancel();

    RebuilderResult Rebuild(RomDatabase db, string sourceFolderPath, string targetFolderPath, RebuildOptions options);
}
