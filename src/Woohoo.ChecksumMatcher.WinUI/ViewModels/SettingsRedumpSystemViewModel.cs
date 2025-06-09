// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public sealed partial class SettingsRedumpSystemViewModel : ObservableObject
{
    private readonly Action? changedHandler;

    public SettingsRedumpSystemViewModel(string id, string name, bool isEnabled, Action? changedHandler)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentException.ThrowIfNullOrEmpty(name);

        this.Id = id;
        this.Name = name;
        this.IsEnabled = isEnabled;

        this.changedHandler = changedHandler;
    }

    public string Id { get; }

    public string Name { get; }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    partial void OnIsEnabledChanged(bool value)
    {
        this.changedHandler?.Invoke();
    }
}
