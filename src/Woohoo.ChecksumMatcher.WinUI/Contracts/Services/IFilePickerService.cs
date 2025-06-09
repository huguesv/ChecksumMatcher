// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public interface IFilePickerService
{
    Task<string?> GetOpenFolderPathAsync(string settingsIdentifier);

    Task<string?> GetOpenFilePathAsync(string settingsIdentifier, params (string Name, string ExtensionList)[] filters);

    Task<string?> GetSaveFilePathAsync(string settingsIdentifier, params (string Name, string ExtensionList)[] filters);
}
