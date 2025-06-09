// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

using System.Collections.Specialized;

public interface IAppNotificationService
{
    void Initialize();

    bool Show(string payload);

    NameValueCollection ParseArguments(string arguments);

    void Unregister();
}
