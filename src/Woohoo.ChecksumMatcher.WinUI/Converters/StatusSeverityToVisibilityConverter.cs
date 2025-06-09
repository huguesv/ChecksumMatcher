// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Converters;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

internal partial class StatusSeverityToVisibilityConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is StatusSeverity severity)
        {
            return severity switch
            {
                StatusSeverity.None => Visibility.Collapsed,
                _ => Visibility.Visible,
            };
        }

        throw new ArgumentException("Value must be of type StatusSeverity", nameof(value));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
