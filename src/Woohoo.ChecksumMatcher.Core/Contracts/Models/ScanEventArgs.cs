// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public sealed class ScanEventArgs : DatabaseEventArgs
{
    public required string OperationId { get; init; }

    public int ProgressPercentage { get; init; }

    public required DatabaseScanResults Results { get; init; }

    public required ScanStatus Status { get; init; }

    public FileMoniker? HashingFile { get; init; }
}
