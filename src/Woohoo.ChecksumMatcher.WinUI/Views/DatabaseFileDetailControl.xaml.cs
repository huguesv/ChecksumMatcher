// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Woohoo.ChecksumMatcher.WinUI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public sealed partial class DatabaseFileDetailControl : UserControl
{
    public DatabaseViewModel? Database
    {
        get => this.GetValue(DatabaseProperty) as DatabaseViewModel;
        set => this.SetValue(DatabaseProperty, value);
    }

    public static readonly DependencyProperty DatabaseProperty = DependencyProperty.Register(nameof(Database), typeof(DatabaseViewModel), typeof(DatabaseFileDetailControl), new PropertyMetadata(null, OnDatabasePropertyChanged));

    public DatabaseFileDetailControl()
    {
        this.InitializeComponent();
    }

    private static void OnDatabasePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatabaseFileDetailControl control)
        {
            //control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
