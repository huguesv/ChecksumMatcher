// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Diagnostics;

[DebuggerDisplay("FolderPath = {FolderPath}")]
public class DatabaseSelectOfflineFolderItemViewModel
{
    public DatabaseSelectOfflineFolderItemViewModel(string folderPath)
    {
        ArgumentNullException.ThrowIfNull(folderPath);

        this.FolderPath = folderPath;
    }

    public string FolderPath { get; }
}
