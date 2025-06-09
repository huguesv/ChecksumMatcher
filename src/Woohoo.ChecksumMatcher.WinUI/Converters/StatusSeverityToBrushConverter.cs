// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Converters;

using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

internal sealed partial class StatusSeverityToBrushConverter : IValueConverter
{
    public Brush? NoneBrush { get; set; }

    public Brush? InfoBrush { get; set; }

    public Brush? WarningBrush { get; set; }

    public Brush? ErrorBrush { get; set; }

    public Brush? SuccessBrush { get; set; }

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is StatusSeverity severity)
        {
            return severity switch
            {
                StatusSeverity.None => this.NoneBrush,
                StatusSeverity.Info => this.InfoBrush,
                StatusSeverity.Warning => this.WarningBrush,
                StatusSeverity.Error => this.ErrorBrush,
                StatusSeverity.Success => this.SuccessBrush,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown status severity"),
            };
        }

        throw new ArgumentException("Value must be of type StatusSeverity", nameof(value));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
