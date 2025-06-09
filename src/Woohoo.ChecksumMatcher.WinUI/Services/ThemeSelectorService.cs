// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using Microsoft.UI.Xaml;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Helpers;

public class ThemeSelectorService : IThemeSelectorService
{
    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    private readonly ILocalSettingsService _localSettingsService;

    public ThemeSelectorService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        this.Theme = this.LoadThemeFromSettings();
        await Task.CompletedTask;
    }

    public async Task SetThemeAsync(ElementTheme theme)
    {
        this.Theme = theme;

        await this.SetRequestedThemeAsync();
        this.SaveThemeInSettings(this.Theme);
    }

    public async Task SetRequestedThemeAsync()
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = this.Theme;

            TitleBarHelper.UpdateTitleBar(this.Theme);
        }

        await Task.CompletedTask;
    }

    private ElementTheme LoadThemeFromSettings()
    {
        var themeName = _localSettingsService.ReadSetting<string>(SettingKeys.AppBackgroundRequestedTheme);

        if (Enum.TryParse(themeName, out ElementTheme cacheTheme))
        {
            return cacheTheme;
        }

        return ElementTheme.Default;
    }

    private void SaveThemeInSettings(ElementTheme theme)
    {
        _localSettingsService.SaveSetting(SettingKeys.AppBackgroundRequestedTheme, theme.ToString());
    }
}
