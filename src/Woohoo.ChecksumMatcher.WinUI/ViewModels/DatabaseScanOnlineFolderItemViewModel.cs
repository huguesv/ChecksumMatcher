// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public sealed partial class DatabaseScanOnlineFolderItemViewModel : SettingsFolderViewModel
{
    private readonly Action<DatabaseScanOnlineFolderItemViewModel, bool> changeIncludeFunc;

    public DatabaseScanOnlineFolderItemViewModel(
        IFileExplorerService fileExplorerService,
        IDispatcherQueue dispatcherQueue,
        ILogger logger,
        string path,
        IAsyncRelayCommand? removeCommand,
        Action<DatabaseScanOnlineFolderItemViewModel, bool> changeIncludeFunc)
        : base(fileExplorerService, dispatcherQueue, logger, path, removeCommand)
    {
        ArgumentNullException.ThrowIfNull(changeIncludeFunc);

        this.changeIncludeFunc = changeIncludeFunc;
    }

    [ObservableProperty]
    public partial bool IsIncluded { get; set; } = true;

    partial void OnIsIncludedChanged(bool value)
    {
        this.changeIncludeFunc(this, value);
    }
}
