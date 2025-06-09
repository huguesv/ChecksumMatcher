// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public sealed partial class DatabaseTargetContainerTypeViewModel : ObservableObject
{
    public required string Type { get; init; }

    public required string DisplayName { get; init; }
}
