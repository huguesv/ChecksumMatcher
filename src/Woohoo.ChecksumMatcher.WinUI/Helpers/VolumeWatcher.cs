// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Helpers;

using System.Management;

internal sealed partial class VolumeWatcher : IDisposable
{
    private readonly WqlEventQuery query;
    private readonly ManagementEventWatcher watcher;

    public VolumeWatcher()
    {
        this.query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 OR EventType = 3");
        this.watcher = new ManagementEventWatcher(this.query);
        this.watcher.EventArrived += this.Watcher_EventArrived;
        this.watcher.Start();
    }

    public event EventHandler? VolumesChanged;

    public void Dispose()
    {
        this.watcher.EventArrived -= this.Watcher_EventArrived;
        this.watcher.Stop();
        this.watcher.Dispose();
    }

    private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
    {
        this.VolumesChanged?.Invoke(this, EventArgs.Empty);
    }
}
