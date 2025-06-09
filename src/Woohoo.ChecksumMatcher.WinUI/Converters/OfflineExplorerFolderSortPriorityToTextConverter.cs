// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Converters;

using System;
using Microsoft.UI.Xaml.Data;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

internal partial class OfflineExplorerFolderSortPriorityToTextConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is OfflineExplorerFolderSortPriority theme)
        {
            switch (theme)
            {
                case OfflineExplorerFolderSortPriority.First:
                    return "Folders before files";
                case OfflineExplorerFolderSortPriority.Last:
                    return "Folders after files";
                case OfflineExplorerFolderSortPriority.Mixed:
                    return "Folders mixed with files";
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
