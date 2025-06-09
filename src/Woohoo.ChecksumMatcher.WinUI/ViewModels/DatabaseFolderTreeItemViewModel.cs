// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Diagnostics;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

[DebuggerDisplay("Name = {Name} Path = {folderPath}")]
public sealed partial class DatabaseFolderTreeItemViewModel : DatabaseTreeItemViewModel
{
    private readonly string folderPath;
    private readonly IClipboardService clipboardService;
    private readonly IFileExplorerService fileExplorerService;

    public DatabaseFolderTreeItemViewModel(
        DatabaseFolderTreeItemViewModel? parentFolderItem,
        DatabaseFolder databaseFolder,
        string name,
        string folderPath,
        DatabaseFolderViewModel folderViewModel,
        IClipboardService clipboardService,
        IFileExplorerService fileExplorerService)
        : base(name, ExplorerItemType.Folder, parentFolderItem)
    {
        ArgumentNullException.ThrowIfNull(databaseFolder);
        ArgumentNullException.ThrowIfNull(folderPath);
        ArgumentNullException.ThrowIfNull(folderViewModel);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);

        this.DatabaseFolder = databaseFolder;
        this.folderPath = folderPath;
        this.FolderViewModel = folderViewModel;
        this.clipboardService = clipboardService;
        this.fileExplorerService = fileExplorerService;
    }

    public DatabaseFolder DatabaseFolder { get; }

    public DatabaseFolderViewModel FolderViewModel { get; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [RelayCommand]
    private void OpenInExplorer()
    {
        this.fileExplorerService.OpenInExplorer(this.folderPath);
    }

    [RelayCommand]
    private void CopyAllMissing()
    {
        var result = new StringBuilder();

        DoCopy(this, result);

        this.clipboardService.SetText(result.ToString());

        static void DoCopy(DatabaseFolderTreeItemViewModel folder, StringBuilder output)
        {
            foreach (var child in folder.Children.OrderBy(f => f.Name))
            {
                if (child is DatabaseFileTreeItemViewModel fileChild && fileChild.HasScanResults)
                {
                    var containerNames = fileChild.MissingFiles.Select(f => f.ContainerName).Order().Distinct().ToArray();

                    output.AppendLine("===============================================================================");
                    output.AppendLine(fileChild.FileName);
                    output.AppendLine(string.Format(CultureInfo.CurrentUICulture, Localized.ScanResultMissingCountFormat, containerNames.Length));
                    output.AppendLine("-------------------------------------------------------------------------------");

                    foreach (var containerName in containerNames)
                    {
                        output.AppendLine(containerName);
                    }

                    output.AppendLine();
                }
                else if (child is DatabaseFolderTreeItemViewModel folderChild)
                {
                    DoCopy(folderChild, output);
                }
            }
        }
    }

    [RelayCommand]
    private void ExpandAllDescendants()
    {
        foreach (var child in this.Children)
        {
            if (child is DatabaseFolderTreeItemViewModel folderChild)
            {
                folderChild.ExpandAllDescendants();
            }
        }

        this.IsExpanded = true;
    }

    [RelayCommand]
    private void CollapseAllDescendants()
    {
        foreach (var child in this.Children)
        {
            if (child is DatabaseFolderTreeItemViewModel folderChild)
            {
                folderChild.CollapseAllDescendants();
            }
        }

        this.IsExpanded = false;
    }
}
