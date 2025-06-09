// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public sealed partial class StatusViewModel : ObservableObject
{
    public StatusViewModel()
        : this(string.Empty, StatusSeverity.Info)
    {
    }

    public StatusViewModel(string text, StatusSeverity severity)
    {
        ArgumentNullException.ThrowIfNull(text);

        this.Text = text;
        this.Severity = severity;
    }

    [ObservableProperty]
    public partial string Text { get; set; }

    [ObservableProperty]
    public partial StatusSeverity Severity { get; set; }
}
