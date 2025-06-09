// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class DatabaseInfoControl : UserControl
{
    public DatabaseInfoControl()
    {
        this.InitializeComponent();
    }

    public DatabaseViewModel? ViewModel
    {
        get => this.GetValue(ViewModelProperty) as DatabaseViewModel;
        set => this.SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(DatabaseViewModel), typeof(DatabaseInfoControl), new PropertyMetadata(null, OnViewModelChanged));

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatabaseInfoControl control)
        {
            //control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
