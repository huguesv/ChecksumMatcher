// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Helpers;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

public class EnumToBooleanConverter : IValueConverter
{
    public EnumToBooleanConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            if (!Enum.IsDefined(typeof(ElementTheme), value))
            {
                throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
            }

            var enumValue = Enum.Parse(typeof(ElementTheme), enumString);

            return enumValue.Equals(value);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            return Enum.Parse(typeof(ElementTheme), enumString);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }
}
