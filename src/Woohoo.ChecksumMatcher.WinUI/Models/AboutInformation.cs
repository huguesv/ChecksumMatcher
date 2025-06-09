// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Models;

public sealed class AboutInformation
{
    public required string AppVersion { get; init; }

    public required string DotNetFramework { get; init; }

    public required string ProcessArchitecture { get; init; }

    public required string RuntimeIdentifier { get; init; }

    public required string OperatingSystem { get; init; }

    public required string WindowsRuntime { get; init; }

    public required string WindowsSdk { get; init; }
}
