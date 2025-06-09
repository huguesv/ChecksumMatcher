// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

public interface IFileService
{
    T? Read<T>(string folderPath, string fileName);

    void Save<T>(string folderPath, string fileName, T content);

    void Delete(string folderPath, string fileName);
}
