// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers;

using System.IO;

internal interface IContainer
{
    string FileExtension { get; }

    Task<FileInformation[]> GetAllFilesAsync(string containerFilePath, SearchOption searchOption, CancellationToken ct);

    Task CalculateChecksumsAsync(FileInformation file, CancellationToken ct);

    Task<bool> ExistsAsync(FileInformation file, CancellationToken ct);

    Task CopyAsync(FileInformation file, string targetFilePath, CancellationToken ct);

    Task MoveAsync(FileInformation file, string targetFilePath, CancellationToken ct);

    Task RemoveAsync(FileInformation file, CancellationToken ct);

    Task RemoveContainerAsync(string containerFilePath, CancellationToken ct);

    /// <summary>
    /// Determines if the container exists and is empty. If it does not exists, it returns <c>false</c>.
    /// </summary>
    Task<bool> IsEmptyContainerAsync(string containerFilePath, CancellationToken ct);
}
