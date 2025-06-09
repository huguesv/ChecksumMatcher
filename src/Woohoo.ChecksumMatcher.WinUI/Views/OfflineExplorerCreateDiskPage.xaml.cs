// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class OfflineExplorerCreateDiskPage : Page
{
    public OfflineExplorerCreateDiskPage()
    {
        this.ViewModel = App.GetService<OfflineExplorerCreateDiskViewModel>();

        this.InitializeComponent();
    }

    public OfflineExplorerCreateDiskViewModel ViewModel { get; }
}
