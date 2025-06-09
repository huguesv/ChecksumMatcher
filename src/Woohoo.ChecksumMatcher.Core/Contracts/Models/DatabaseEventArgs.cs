// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

using System;
using Woohoo.ChecksumDatabase.Model;

public class DatabaseEventArgs : EventArgs
{
    public required DatabaseFile DatabaseFile { get; init; }

    public RomDatabase? Database { get; init; }
}
