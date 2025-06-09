// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Converter;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

internal class TextEqualsToVisibilityConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string actual && parameter is string desired)
        {
            return actual == desired ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
