// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Converters;

using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

internal partial class IntegerToFileSizeTextConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is long size1)
        {
            return ConvertToText(size1);
        }

        if (value is int size2)
        {
            return ConvertToText(size2);
        }

        if (value is short size3)
        {
            return ConvertToText(size3);
        }

        if (value is ulong size4)
        {
            return ConvertToText(size4);
        }

        if (value is uint size5)
        {
            return ConvertToText(size5);
        }

        if (value is ushort size6)
        {
            return ConvertToText(size6);
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    private static string ConvertToText(long value)
    {
        if (value < 10_000)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} B", value);
        }

        if (value < 10_000_000)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} KB", value / 1024L);
        }

        if (value < 10_000_000_000)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} MB", value / (1024L * 1024L));
        }

        if (value < 10_000_000_000_000)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} GB", value / (1024L * 1024L * 1024L));
        }

        return string.Format(CultureInfo.CurrentCulture, "{0} TB", value / (1024L * 1024L * 1024L * 1024L));
    }

    private static string ConvertToText(ulong value)
    {
        if (value < 10_000)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} B", value);
        }

        if (value < 10_000_000)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} KB", value / 1024);
        }

        if (value < 10_000_000_000)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} MB", value / (1024 * 1024));
        }

        return string.Format(CultureInfo.CurrentCulture, "{0} GB", value / (1024 * 1024 * 1024));
    }
}
