// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using Windows.ApplicationModel.DataTransfer;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

class ClipboardService : IClipboardService
{
    public void SetText(string text)
    {
        DataPackage dataPackage = new()
        {
            RequestedOperation = DataPackageOperation.Copy,
        };
        dataPackage.SetText(text);
        Clipboard.SetContent(dataPackage);
    }
}
