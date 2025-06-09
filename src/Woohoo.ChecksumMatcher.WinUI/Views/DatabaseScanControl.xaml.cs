// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class DatabaseScanControl : UserControl
{
    public DatabaseScanControl()
    {
        this.InitializeComponent();
    }

    public DatabaseViewModel? ViewModel
    {
        get => this.GetValue(ViewModelProperty) as DatabaseViewModel;
        set => this.SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(DatabaseViewModel), typeof(DatabaseScanControl), new PropertyMetadata(null, OnViewModelChanged));

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatabaseInfoControl control)
        {
            //control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
