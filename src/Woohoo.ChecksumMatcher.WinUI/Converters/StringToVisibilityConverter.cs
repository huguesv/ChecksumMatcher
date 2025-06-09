// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Converters;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

internal sealed partial class StringToVisibilityConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string str && !string.IsNullOrEmpty(str))
        {
            return ToVisibility(true, parameter);
        }

        return ToVisibility(false, parameter);

        static Visibility ToVisibility(bool value, object parameter)
        {
            if (parameter is bool invert && invert)
            {
                return value ? Visibility.Collapsed : Visibility.Visible;
            }
            else if (parameter is string text && string.Equals(text, true.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return value ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
