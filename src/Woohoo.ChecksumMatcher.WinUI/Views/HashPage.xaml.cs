// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class HashPage : Page
{
    public HashViewModel ViewModel
    {
        get;
    }

    public HashPage()
    {
        this.ViewModel = App.GetService<HashViewModel>();
        this.InitializeComponent();
    }
}
