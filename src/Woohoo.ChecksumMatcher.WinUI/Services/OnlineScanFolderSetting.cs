// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

public record class OnlineScanFolderSetting
{
    public string FolderPath { get; set; } = string.Empty;

    public bool IsIncluded { get; set; } = true;
}
