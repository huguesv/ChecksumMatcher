// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Internal.SevenZip;

using System.IO;
using global::SevenZip;
using Woohoo.IO.Compression.SevenZip;

internal class FolderToSevenZipCopier : IFileCopier
{
    static FolderToSevenZipCopier()
    {
        SevenZipLibrary.Initialize();
    }

    protected virtual bool Compress => true;

    public virtual int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetContainerType);

        if (Directory.Exists(file.ContainerAbsolutePath))
        {
            if (targetContainerType == "7z")
            {
                return 5;
            }
        }

        return 0;
    }

    public virtual bool Copy(FileInformation file, string targetFolderPath, bool removeSource, bool allowContainerMove, string containerName, string fileName, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(containerName);
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        var targetArchiveFilePath = Path.Combine(targetFolderPath, containerName + ".7z");
        this.CopyFromFileToZipArchive(Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath), targetArchiveFilePath, fileName);

        // if (!_cancel)
        return this.PostProcess(targetArchiveFilePath, expectedTargetFiles);
    }

    public virtual string GetTargetContainerPath(string targetFolderPath, string containerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(containerName);

        return Path.Combine(targetFolderPath, containerName + ".zip");
    }

    internal static void BuildArchive(string sourceFilePath, string targetArchiveFilePath, string targetFile)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFilePath);
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);
        ArgumentException.ThrowIfNullOrEmpty(targetFile);

        var compressor = new SevenZipCompressor
        {
            CompressionMode = File.Exists(targetArchiveFilePath) ? CompressionMode.Append : CompressionMode.Create,
            CompressionMethod = CompressionMethod.Copy,
            ArchiveFormat = OutArchiveFormat.SevenZip,
            DefaultItemName = targetFile,
        };

        using var inStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var input = new Dictionary<string, Stream>
        {
            [targetFile] = inStream,
        };

        compressor.CompressStreamDictionary(input, targetArchiveFilePath);
    }

    protected virtual bool PostProcess(string targetArchiveFilePath, string[] expectedTargetFiles)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        return SevenZipContainer.IsComplete(targetArchiveFilePath, expectedTargetFiles);
    }

    protected virtual void CopyFromFileToZipArchive(string sourceFilePath, string targetArchiveFilePath, string targetFile)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFilePath);
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);
        ArgumentException.ThrowIfNullOrEmpty(targetFile);

        BuildArchive(sourceFilePath, targetArchiveFilePath, targetFile);
    }
}
