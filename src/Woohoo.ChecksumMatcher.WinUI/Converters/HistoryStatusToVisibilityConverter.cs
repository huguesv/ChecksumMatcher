// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Converters;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

internal sealed partial class HistoryStatusToVisibilityConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is HistoryStatus status)
        {
            switch (status)
            {
                case HistoryStatus.Pending:
                case HistoryStatus.InProgress:
                    return Visibility.Visible;
                default:
                    return Visibility.Collapsed;
            }
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
