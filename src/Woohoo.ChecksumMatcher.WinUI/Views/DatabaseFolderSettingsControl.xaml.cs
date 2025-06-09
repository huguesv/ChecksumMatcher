// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class DatabaseFolderSettingsControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(DatabaseFolderViewModel), typeof(DatabaseFolderSettingsControl), new PropertyMetadata(null, OnViewModelChanged));

    public DatabaseFolderSettingsControl()
    {
        this.InitializeComponent();

        this.ApplyOfflineFolderButton.Click += this.ApplyOfflineFolderButton_Click;
    }

    public DatabaseFolderViewModel? ViewModel
    {
        get => this.GetValue(ViewModelProperty) as DatabaseFolderViewModel;
        set => this.SetValue(ViewModelProperty, value);
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatabaseFolderSettingsControl control)
        {
        }
    }

    private void ApplyOfflineFolderButton_Click(object sender, RoutedEventArgs e)
    {
        this.AddOfflineFolderFlyout.Hide();
    }

    private void AddOfflineFolderFlyout_Opening(object sender, object e)
    {
        this.ViewModel?.BrowseOfflineFolder.EnsureLoadedAsync(CancellationToken.None).SafeFireAndForget();
    }
}
