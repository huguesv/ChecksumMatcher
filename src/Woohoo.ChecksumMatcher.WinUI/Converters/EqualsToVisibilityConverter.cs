// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Converters;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

internal partial class EqualsToVisibilityConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        return EqualityComparer<object?>.Default.Equals(value, parameter) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
