// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using System.Threading.Tasks;
using Microsoft.Windows.AppNotifications.Builder;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Models;

public class OperationCompletionService : IOperationCompletionService
{
    private readonly ILocalSettingsService localSettingsService;
    private readonly ISoundService soundService;
    private readonly IAppNotificationService appNotificationService;

    public OperationCompletionService(ILocalSettingsService localSettingsService, ISoundService soundService, IAppNotificationService appNotificationService)
    {
        this.localSettingsService = localSettingsService;
        this.soundService = soundService;
        this.appNotificationService = appNotificationService;
    }

    public async Task NotifyCompletion(OperationCompletionResult result, string message, CancellationToken ct)
    {
        await this.PlaySound(result, ct);
        this.ShowNotification(result, message);
    }

    public async Task NotifyCompletionWithOpenInExplorer(OperationCompletionResult result, string message, string filePath, CancellationToken ct)
    {
        await this.PlaySound(result, ct);
        this.ShowNotification(result, message, filePath);
    }

    private async Task PlaySound(OperationCompletionResult result, CancellationToken ct)
    {
        bool playSound = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.UseSystemSounds) ?? false;
        if (playSound)
        {
            switch (result)
            {
                case OperationCompletionResult.Success:
                    await this.soundService.PlayAsync(SystemSound.Success, ct);
                    break;
                case OperationCompletionResult.Error:
                    await this.soundService.PlayAsync(SystemSound.Error, ct);
                    break;
                case OperationCompletionResult.Cancelled:
                    await this.soundService.PlayAsync(SystemSound.Error, ct);
                    break;
                default:
                    break;
            }
        }
    }

    private void ShowNotification(OperationCompletionResult result, string message)
    {
        bool sendNotification = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.UseWindowsNotifications) ?? true;
        if (sendNotification)
        {
            var builder = new AppNotificationBuilder()
                .AddText(message);

            this.appNotificationService.Show(builder.BuildNotification());
        }
    }

    private void ShowNotification(OperationCompletionResult result, string message, string filePath)
    {
        bool sendNotification = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.UseWindowsNotifications) ?? true;
        if (sendNotification)
        {
            var builder = new AppNotificationBuilder()
                .AddText(message)
                .AddButton(new AppNotificationButton(Localized.AppNotificationOpenInExplorer)
                    .AddArgument(AppNotificationArgumentNames.Action, AppNotificationArgumentActions.OpenExplorer)
                    .AddArgument(AppNotificationArgumentNames.File, filePath));

            this.appNotificationService.Show(builder.BuildNotification());
        }
    }
}
