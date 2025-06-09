// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.ChecksumMatcher.WinUI.Activation;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Views;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> defaultHandler;
    private readonly IEnumerable<IActivationHandler> activationHandlers;
    private readonly IThemeSelectorService themeSelectorService;
    private UIElement? shell = null;

    public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, IEnumerable<IActivationHandler> activationHandlers, IThemeSelectorService themeSelectorService)
    {
        this.defaultHandler = defaultHandler;
        this.activationHandlers = activationHandlers;
        this.themeSelectorService = themeSelectorService;
    }

    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await this.InitializeAsync();

        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
        {
            this.shell = App.GetService<ShellPage>();
            App.MainWindow.Content = this.shell ?? new Frame();
        }

        // Handle activation via ActivationHandlers.
        await this.HandleActivationAsync(activationArgs);

        // Activate the MainWindow.
        App.MainWindow.Activate();

        // Execute tasks after activation.
        await this.StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        var activationHandler = this.activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(activationArgs);
        }

        if (this.defaultHandler.CanHandle(activationArgs))
        {
            await this.defaultHandler.HandleAsync(activationArgs);
        }
    }

    private async Task InitializeAsync()
    {
        await this.themeSelectorService.InitializeAsync();
    }

    private async Task StartupAsync()
    {
        await this.themeSelectorService.SetRequestedThemeAsync();
    }
}
