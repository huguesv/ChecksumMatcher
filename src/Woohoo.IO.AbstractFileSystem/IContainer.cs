// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

using System.IO;

public interface IContainer
{
    string FileExtension { get; }

    FileInformation[] GetAllFiles(string containerFilePath, SearchOption searchOption);

    void CalculateChecksums(FileInformation file);

    bool Exists(FileInformation file);

    void Copy(FileInformation file, string targetFilePath);

    void Move(FileInformation file, string targetFilePath);

    void Remove(FileInformation file);
}
