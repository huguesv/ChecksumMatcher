// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

[DebuggerDisplay("Name = {Name}")]
public sealed partial class DatabaseTreeItemBreadcrumbViewModel : ObservableObject
{
    public DatabaseTreeItemBreadcrumbViewModel(DatabaseTreeItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);

        this.Item = item;
    }

    public string Name => this.Item is DatabaseFileTreeItemViewModel f ? f.FileName : this.Item.Name;

    public DatabaseTreeItemViewModel Item { get; }
}
