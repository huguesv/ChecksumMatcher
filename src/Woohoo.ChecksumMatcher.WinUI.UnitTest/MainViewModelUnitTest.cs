// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest;

using System;
using System.Threading.Tasks;
using Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public class MainViewModelUnitTest
{
    [UIFact]
    public async Task OpenWebBrowserNoIntro()
    {
        // Arrange
        var webLauncherService = new TestWebLauncherService();
        var sut = new MainViewModel(webLauncherService, new TestLogger<MainViewModel>());

        // Act
        await sut.NoIntroWebSite.OpenWebBrowserCommand.ExecuteAsync(null);

        // Arrange
        sut.WebSites.Should().HaveCount(3);
        webLauncherService.CurrentUri.Should().BeEquivalentTo(new Uri("https://no-intro.org/"));
    }

    [UIFact]
    public async Task OpenWebBrowserRedump()
    {
        // Arrange
        var webLauncherService = new TestWebLauncherService();
        var sut = new MainViewModel(webLauncherService, new TestLogger<MainViewModel>());

        // Act
        await sut.RedumpWebSite.OpenWebBrowserCommand.ExecuteAsync(null);

        // Arrange
        sut.WebSites.Should().HaveCount(3);
        webLauncherService.CurrentUri.Should().BeEquivalentTo(new Uri("http://www.redump.org/"));
    }

    [UIFact]
    public async Task OpenWebBrowserTosec()
    {
        // Arrange
        var webLauncherService = new TestWebLauncherService();
        var sut = new MainViewModel(webLauncherService, new TestLogger<MainViewModel>());

        // Act
        await sut.TosecWebSite.OpenWebBrowserCommand.ExecuteAsync(null);

        // Arrange
        sut.WebSites.Should().HaveCount(3);
        webLauncherService.CurrentUri.Should().BeEquivalentTo(new Uri("https://www.tosecdev.org/"));
    }
}
