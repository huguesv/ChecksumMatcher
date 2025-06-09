// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Converters;

using System;
using Microsoft.UI.Xaml.Data;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

internal partial class RomStatusToTextConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DatabaseRomStatus status)
        {
            switch (status)
            {
                case DatabaseRomStatus.Unknown:
                    return "Unknown";
                case DatabaseRomStatus.Found:
                    return "Found";
                case DatabaseRomStatus.NotFound:
                    return "Not Found";
                case DatabaseRomStatus.FoundWrongName:
                    return "Wrong Name";
                default:
                    return string.Empty;
            }
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
