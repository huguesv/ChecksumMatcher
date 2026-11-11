// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class OfflineStorageFolderSelectionPage : Page
{
    public OfflineStorageFolderSelectionPage()
    {
        this.ViewModel = App.GetService<OfflineStorageFolderSelectionViewModel>();

        this.InitializeComponent();
    }

    public OfflineStorageFolderSelectionViewModel ViewModel { get; }
}
