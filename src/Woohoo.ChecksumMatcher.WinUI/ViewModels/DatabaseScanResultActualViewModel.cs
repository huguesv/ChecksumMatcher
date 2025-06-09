// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public sealed partial class DatabaseScanResultActualViewModel : ObservableObject
{
    public required string ActualContainerAbsolutePath { get; init; }

    public required string ActualContainerName { get; init; }

    public required string ActualFileRelativePath { get; init; }

    public required bool IsFromOfflineStorage { get; init; }

    public string ActualDisplayName => $"{this.ActualContainerName}\\{this.ActualFileRelativePath}";
}
