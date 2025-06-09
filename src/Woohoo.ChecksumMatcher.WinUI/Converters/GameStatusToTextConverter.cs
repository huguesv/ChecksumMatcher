// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Converters;

using System;
using Microsoft.UI.Xaml.Data;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

internal sealed partial class GameStatusToTextConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DatabaseGameStatus status)
        {
            switch (status)
            {
                case DatabaseGameStatus.Unknown:
                    return "Unknown";
                case DatabaseGameStatus.Complete:
                    return "Complete";
                case DatabaseGameStatus.CompleteIncorrectName:
                    return "Complete / Wrong Name";
                case DatabaseGameStatus.Partial:
                    return "Partial";
                case DatabaseGameStatus.PartialIncorrectName:
                    return "Partial / Wrong Name";
                case DatabaseGameStatus.Missing:
                    return "Missing";
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
