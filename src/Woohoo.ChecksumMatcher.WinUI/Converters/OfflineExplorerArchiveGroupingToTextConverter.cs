// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Converters;

using System;
using Microsoft.UI.Xaml.Data;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

internal sealed partial class OfflineExplorerArchiveGroupingToTextConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is OfflineExplorerArchiveGrouping grouping)
        {
            switch (grouping)
            {
                case OfflineExplorerArchiveGrouping.WithFiles:
                    return "Group with files";
                case OfflineExplorerArchiveGrouping.WithFolders:
                    return "Group with folders";
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
