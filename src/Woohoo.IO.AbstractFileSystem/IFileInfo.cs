// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

public interface IFileInfo : IFileSystemInfo
{
    IDirectoryInfo? Directory { get; }

    string? DirectoryName { get; }

    bool IsReadOnly { get; set; }

    long Length { get; }

    System.IO.StreamWriter AppendText();

    IFileInfo CopyTo(string destFileName);

    System.IO.Stream Create();

    System.IO.StreamWriter CreateText();

    void Decrypt();

    void Encrypt();

    void MoveTo(string destFileName);

    void MoveTo(string destFileName, bool overwrite);

    System.IO.Stream Open(System.IO.FileMode mode);

    System.IO.Stream Open(System.IO.FileStreamOptions options);

    System.IO.Stream Open(System.IO.FileMode mode, System.IO.FileAccess access);

    System.IO.Stream Open(System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share);

    System.IO.Stream OpenRead();

    System.IO.StreamReader OpenText();

    System.IO.FileStream OpenWrite();

    IFileInfo Replace(string destinationFileName, string? destinationBackupFileName);

    IFileInfo Replace(string destinationFileName, string? destinationBackupFileName, bool ignoreMetadataErrors);
}
