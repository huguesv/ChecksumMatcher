// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class DatabaseFolderDetailControl : UserControl
{
    public static readonly DependencyProperty DatabaseFolderProperty = DependencyProperty.Register(nameof(DatabaseFolder), typeof(DatabaseFileViewModel), typeof(DatabaseFolderDetailControl), new PropertyMetadata(null, OnDatabaseFolderPropertyChanged));

    public DatabaseFolderDetailControl()
    {
        this.InitializeComponent();
    }

    public DatabaseFolderViewModel? DatabaseFolder
    {
        get => this.GetValue(DatabaseFolderProperty) as DatabaseFolderViewModel;
        set => this.SetValue(DatabaseFolderProperty, value);
    }

    private static void OnDatabaseFolderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatabaseFolderDetailControl control)
        {
        }
    }
}
