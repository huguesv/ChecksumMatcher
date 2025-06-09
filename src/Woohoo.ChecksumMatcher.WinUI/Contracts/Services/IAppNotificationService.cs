// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

using Microsoft.Windows.AppNotifications;

public interface IAppNotificationService
{
    void Initialize();

    bool Show(AppNotification notification);

    void Unregister();
}
