// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using Microsoft.Extensions.Options;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Models;

internal sealed class LocalSettingsService : ILocalSettingsService
{
    private const string DefaultApplicationDataFolder = "Woohoo.ChecksumMatcher.WinUI/ApplicationData";
    private const string DefaultLocalSettingsFile = "LocalSettings.json";

    private readonly IFileService fileService;
    private readonly LocalSettingsOptions options;

    private readonly string localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string applicationDataFolder;
    private readonly string localsettingsFile;

    private IDictionary<string, string> settings;

    private bool isInitialized;

    public LocalSettingsService(IFileService fileService, IOptions<LocalSettingsOptions> options)
    {
        this.fileService = fileService;
        this.options = options.Value;

        this.applicationDataFolder = Path.Combine(this.localApplicationData, this.options.ApplicationDataFolder ?? DefaultApplicationDataFolder);
        this.localsettingsFile = this.options.LocalSettingsFile ?? DefaultLocalSettingsFile;

        this.settings = new Dictionary<string, string>();
    }

    public event EventHandler<SettingChangedEventArgs>? SettingChanged;

    public T? ReadSetting<T>(string key)
    {
        this.Initialize();

        if (this.settings != null && this.settings.TryGetValue(key, out var obj))
        {
            return Json.ToObject<T>(obj);
        }

        return default;
    }

    public void SaveSetting<T>(string key, T value)
    {
        this.Initialize();

        this.settings[key] = Json.Stringify(value);

        this.fileService.Save(this.applicationDataFolder, this.localsettingsFile, this.settings);

        this.SettingChanged?.Invoke(this, new SettingChangedEventArgs(key));
    }

    private void Initialize()
    {
        if (!this.isInitialized)
        {
            var loaded = this.fileService.Read<IDictionary<string, string>>(this.applicationDataFolder, this.localsettingsFile);
            if (loaded is not null)
            {
                this.settings = loaded;
            }

            this.isInitialized = true;
        }
    }
}
