// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Diagnostics;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

[DebuggerDisplay("Name = {Name} Path = {folderPath}")]
public sealed partial class DatabaseFolderTreeItemViewModel : DatabaseTreeItemViewModel
{
    private readonly string folderPath;
    private readonly IClipboardService clipboardService;
    private readonly IFileExplorerService fileExplorerService;
    private readonly ILogger logger;

    public DatabaseFolderTreeItemViewModel(
        DatabaseFolderTreeItemViewModel? parentFolderItem,
        DatabaseFolder databaseFolder,
        string name,
        string folderPath,
        DatabaseFolderViewModel folderViewModel,
        IClipboardService clipboardService,
        IFileExplorerService fileExplorerService,
        ILogger logger)
        : base(name, ExplorerItemType.Folder, parentFolderItem)
    {
        ArgumentNullException.ThrowIfNull(databaseFolder);
        ArgumentNullException.ThrowIfNull(folderPath);
        ArgumentNullException.ThrowIfNull(folderViewModel);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);
        ArgumentNullException.ThrowIfNull(logger);

        this.DatabaseFolder = databaseFolder;
        this.folderPath = folderPath;
        this.FolderViewModel = folderViewModel;
        this.clipboardService = clipboardService;
        this.fileExplorerService = fileExplorerService;
        this.logger = logger;
    }

    public DatabaseFolder DatabaseFolder { get; }

    public DatabaseFolderViewModel FolderViewModel { get; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [RelayCommand]
    private void OpenInExplorer()
    {
        try
        {
            this.fileExplorerService.OpenInExplorer(this.folderPath);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private void CopyAllMissing()
    {
        try
        {
            var result = new StringBuilder();

            DoCopy(this, result);

            this.clipboardService.SetText(result.ToString());
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }

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
        try
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
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private void CollapseAllDescendants()
    {
        try
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
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }
}
