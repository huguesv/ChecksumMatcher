// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

[DebuggerDisplay("Name = {Name}")]
public sealed partial class OfflineExplorerDriveViewModel : ObservableObject
{
    private readonly DriveInfo driveInfo;
    private readonly IDispatcherQueue dispatcherQueue;
    private readonly ILogger logger;

    public OfflineExplorerDriveViewModel(DriveInfo driveInfo, IDispatcherQueue dispatcherQueue, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(driveInfo);
        ArgumentNullException.ThrowIfNull(dispatcherQueue);
        ArgumentNullException.ThrowIfNull(logger);

        this.driveInfo = driveInfo;
        this.dispatcherQueue = dispatcherQueue;
        this.logger = logger;

        // Label fetching can block for a long time (in case of unavailable
        // mapped network shares), so we do it in a worker thread.
        this.LoadLabelAsync(CancellationToken.None).FireAndForget((ex) => this.logger.LogError(ex, "Error loading label."));
    }

    public string Name => this.driveInfo.Name;

    [ObservableProperty]
    public partial string Label { get; set; } = string.Empty;

    public bool IsReady => this.driveInfo.IsReady;

    public string FullPath => this.driveInfo.RootDirectory.FullName;

    private async Task LoadLabelAsync(CancellationToken ct)
    {
        await Task.Run(DoLoad, ct);

        void DoLoad()
        {
            var label = this.driveInfo.IsReady ? this.driveInfo.VolumeLabel : string.Empty;
            this.dispatcherQueue.TryEnqueue(() =>
            {
                this.Label = label;
            });
        }
    }
}
