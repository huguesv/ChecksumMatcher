// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class DatabaseLibraryPage : Page
{
    public DatabaseLibraryPage()
    {
        this.ViewModel = App.GetService<DatabaseLibraryViewModel>();
        this.InitializeComponent();
    }

    public DatabaseLibraryViewModel ViewModel { get; }
}
