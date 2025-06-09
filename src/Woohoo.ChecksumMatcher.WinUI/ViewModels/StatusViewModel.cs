// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public partial class StatusViewModel(string text, StatusSeverity severity) : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; } = text;

    [ObservableProperty]
    public partial StatusSeverity Severity { get; set; } = severity;
}
