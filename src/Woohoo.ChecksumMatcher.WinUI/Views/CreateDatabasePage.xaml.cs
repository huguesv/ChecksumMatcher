// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Woohoo.ChecksumMatcher.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateDatabasePage : Page
    {
        public CreateDatabasePage()
        {
            this.ViewModel = App.GetService<CreateDatabaseViewModel>();

            this.InitializeComponent();
        }

        public CreateDatabaseViewModel ViewModel
        {
            get;
        }

    }
}
