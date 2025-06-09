// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public record class HistoryItem
{
    public required string Id { get; init; }

    public required DateTimeOffset StartTime { get; init; }

    public DateTimeOffset? EndTime { get; init; }

    public required HistoryItemStatus Status { get; init; }

    public required string Title { get; init; }

    public required string Subtitle { get; init; }

    public string? Details { get; init; }

    public bool PreventQuit { get; init; }

    public string? NavigationPage { get; init; }

    public string? NavigationParameter { get; init; }
}
