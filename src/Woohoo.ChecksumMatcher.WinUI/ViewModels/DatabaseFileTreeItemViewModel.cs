// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

public class DatabaseFileTreeItemViewModel : DatabaseTreeItemViewModel
{
    public DatabaseFileTreeItemViewModel(DatabaseFolderTreeItemViewModel parentFolderItem, string name, string rootFolder, string absoluteFilePath, string relativeFilePath, DatabaseViewModel? database)
        : base(name, ExplorerItemType.File)
    {
        this.ParentFolderItem = parentFolderItem;
        this.RootFolder = rootFolder;
        this.AbsoluteFilePath = absoluteFilePath;
        this.RelativeFilePath = relativeFilePath;
        this.FileName = Path.GetFileNameWithoutExtension(absoluteFilePath);
        this.Database = database;
    }

    public DatabaseFolderTreeItemViewModel ParentFolderItem { get; }

    public string RootFolder { get; }

    public string AbsoluteFilePath { get; }

    public string RelativeFilePath { get; }

    public string FileName { get; }

    public DatabaseViewModel? Database { get; }
}
