// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class DatabasesPage : Page
{
    public DatabasesViewModel ViewModel
    {
        get;
    }

    public DatabasesPage()
    {
        this.ViewModel = App.GetService<DatabasesViewModel>();
        this.InitializeComponent();
    }

    private void TreeView1_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
    {
    }

    //private void OnViewStateChanged(object sender, ListDetailsViewState e)
    //{
    //    if (e == ListDetailsViewState.Both)
    //    {
    //        ViewModel.EnsureItemSelected();
    //    }
    //}
}
