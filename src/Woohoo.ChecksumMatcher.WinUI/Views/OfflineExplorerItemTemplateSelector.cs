// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

class OfflineExplorerItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? FolderTemplate
    {
        get; set;
    }

    public DataTemplate? ArchiveTemplate
    {
        get; set;
    }

    public DataTemplate? FileTemplate
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
        if (item is OfflineExplorerFolderViewModel itemViewModel)
        {
            return (itemViewModel.Kind) switch
            {
                OfflineItemKind.ArchiveFile => this.ArchiveTemplate,
                OfflineItemKind.File => this.FileTemplate,
                OfflineItemKind.Folder => this.FolderTemplate,
                _ => throw new Exception($"No template found for {item.GetType()}.")
            };
        }
        else if (item is OfflineExplorerFileViewModel fileViewModel)
        {
            return this.FileTemplate;
        }

        return this.NoSelectionTemplate;
    }
}
