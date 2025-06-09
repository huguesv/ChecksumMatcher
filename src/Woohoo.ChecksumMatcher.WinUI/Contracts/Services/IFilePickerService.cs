// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public interface IFilePickerService
{
    Task<string?> GetOpenFolderPathAsync();

    Task<string?> GetOpenFilePathAsync(params (string Name, string ExtensionList)[] filters);

    Task<string?> GetSaveFilePathAsync(params (string Name, string ExtensionList)[] filters);
}
