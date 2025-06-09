// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class DatabaseFileDetailControl : UserControl
{
    public static readonly DependencyProperty DatabaseProperty = DependencyProperty.Register(nameof(Database), typeof(DatabaseFileViewModel), typeof(DatabaseFileDetailControl), new PropertyMetadata(null, OnDatabasePropertyChanged));

    public DatabaseFileDetailControl()
    {
        this.InitializeComponent();
    }

    public DatabaseFileViewModel? Database
    {
        get => this.GetValue(DatabaseProperty) as DatabaseFileViewModel;
        set => this.SetValue(DatabaseProperty, value);
    }

    private static void OnDatabasePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatabaseFileDetailControl control)
        {
        }
    }
}
