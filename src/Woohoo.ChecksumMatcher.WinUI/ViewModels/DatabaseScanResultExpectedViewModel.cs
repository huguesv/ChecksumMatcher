// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public sealed partial class DatabaseScanResultExpectedViewModel : ObservableObject
{
    public required string ExpectedContainerName { get; init; }

    public required string ExpectedFileRelativePath { get; init; }

    public string ExpectedDisplayName => $"{this.ExpectedContainerName}\\{this.ExpectedFileRelativePath}";
}
