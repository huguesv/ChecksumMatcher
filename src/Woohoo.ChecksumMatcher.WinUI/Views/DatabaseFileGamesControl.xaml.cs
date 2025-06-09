// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class DatabaseFileGamesControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(DatabaseFileViewModel), typeof(DatabaseFileGamesControl), new PropertyMetadata(null, OnViewModelChanged));

    public DatabaseFileGamesControl()
    {
        this.InitializeComponent();
    }

    public DatabaseFileViewModel? ViewModel
    {
        get => this.GetValue(ViewModelProperty) as DatabaseFileViewModel;
        set => this.SetValue(ViewModelProperty, value);
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatabaseFileGamesControl control)
        {
        }
    }
}
