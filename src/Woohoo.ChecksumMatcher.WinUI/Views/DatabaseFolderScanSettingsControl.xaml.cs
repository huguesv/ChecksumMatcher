// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class DatabaseFolderScanSettingsControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(DatabaseFolderViewModel), typeof(DatabaseFolderScanSettingsControl), new PropertyMetadata(null, OnViewModelChanged));

    public DatabaseFolderScanSettingsControl()
    {
        this.InitializeComponent();
    }

    public DatabaseFolderViewModel? ViewModel
    {
        get => this.GetValue(ViewModelProperty) as DatabaseFolderViewModel;
        set => this.SetValue(ViewModelProperty, value);
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatabaseFolderScanSettingsControl control)
        {
        }
    }
}
