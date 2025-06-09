// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Converters;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

internal sealed partial class ElementThemeToTextConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ElementTheme theme)
        {
            switch (theme)
            {
                case ElementTheme.Default:
                    return "Use system setting";
                case ElementTheme.Light:
                    return "Light";
                case ElementTheme.Dark:
                    return "Dark";
                default:
                    break;
            }
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
