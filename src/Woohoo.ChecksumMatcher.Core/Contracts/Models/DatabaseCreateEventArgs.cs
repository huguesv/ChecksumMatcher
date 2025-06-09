// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

using System;

public sealed class DatabaseCreateEventArgs : EventArgs
{
    public required string OperationId { get; init; }

    public required string DatabaseName { get; init; }

    public int ProgressPercentage { get; init; }

    public required DatabaseCreateResults Results { get; init; }

    public required DatabaseCreateStatus Status { get; init; }

    public FileMoniker? HashingFile { get; init; }
}
