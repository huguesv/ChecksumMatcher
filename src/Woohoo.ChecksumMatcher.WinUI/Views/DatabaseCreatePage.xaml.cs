// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Views;

using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public sealed partial class DatabaseCreatePage : Page
{
    public DatabaseCreatePage()
    {
        this.ViewModel = App.GetService<DatabaseCreateViewModel>();

        this.InitializeComponent();

        // Add commands without losing the standard TextBox
        // commands such as Cut, Copy, Paste, etc.
        this.VersionTextBox.ContextFlyout.Opening += this.ContextFlyout_Opening;
    }

    public DatabaseCreateViewModel ViewModel { get; }

    private void ContextFlyout_Opening(object? sender, object e)
    {
        var flyout = sender as TextCommandBarFlyout;
        if (flyout is not null && flyout.Target == this.VersionTextBox)
        {
            var separator = new AppBarSeparator();
            var insertDateNoIntro = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Calendar),
                Label = Localized.CreateDatabaseInsertDateTimeNoIntro,
                Command = this.ViewModel.InsertDateNoIntroCommand,
            };

            var insertDateRedump = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Calendar),
                Label = Localized.CreateDatabaseInsertDateTimeRedump,
                Command = this.ViewModel.InsertDateRedumpCommand,
            };

            flyout.SecondaryCommands.Add(separator);
            flyout.SecondaryCommands.Add(insertDateNoIntro);
            flyout.SecondaryCommands.Add(insertDateRedump);
        }
    }
}
