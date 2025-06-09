// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public class DatabaseCreateSettings
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public string Date { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Homepage { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public string Comment { get; init; } = string.Empty;

    public bool ForceCalculateChecksums { get; init; } = true;
}
