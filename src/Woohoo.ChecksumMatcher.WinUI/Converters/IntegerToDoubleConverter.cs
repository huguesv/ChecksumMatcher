// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Converters;

using System;
using Microsoft.UI.Xaml.Data;

internal partial class IntegerToDoubleConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        return System.Convert.ToDouble(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
