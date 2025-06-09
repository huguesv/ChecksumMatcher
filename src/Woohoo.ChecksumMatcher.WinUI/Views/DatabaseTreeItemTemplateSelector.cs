// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

class DatabaseTreeItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? FolderTemplate
    {
        get; set;
    }

    public DataTemplate? FileTemplate
    {
        get; set;
    }

    public DataTemplate? FileNotLoadedTemplate
    {
        get; set;
    }

    public DataTemplate? NoSelectionTemplate
    {
        get; set;
    }

    protected override DataTemplate? SelectTemplateCore(object? item, DependencyObject container)
    {
        return this.SelectTemplateCore(item);
    }

    protected override DataTemplate? SelectTemplateCore(object? item)
    {
        if (item is null)
        {
            return this.NoSelectionTemplate;
        }

        if (item is DatabaseFolderTreeItemViewModel)
        {
            return this.FolderTemplate;
        }

        if (item is DatabaseFileTreeItemViewModel fileItem)
        {
            return fileItem.Database is not null ? this.FileTemplate : this.FileNotLoadedTemplate;
        }

        throw new Exception($"No template found for {item.GetType()}.");
    }
}
