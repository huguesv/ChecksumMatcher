// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers;

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A container that caches checksum calculations to avoid redundant work.
/// Can wrap any other container.
/// </summary>
internal class CachingContainer : IContainer
{
    private readonly IContainer inner;
    private readonly ConcurrentDictionary<string, CacheItem> cache = new();

    public CachingContainer(IContainer inner)
    {
        ArgumentNullException.ThrowIfNull(inner);

        this.inner = inner;
    }

    public string FileExtension => this.inner.FileExtension;

    public async Task CalculateChecksumsAsync(FileInformation file, CancellationToken ct)
    {
        DateTime? lastWriteUtc = GetLastWriteUtc(file);
        string cacheKey = MakeCacheKey(file);

        if (lastWriteUtc is not null &&
            this.cache.TryGetValue(cacheKey, out CacheItem? cacheItem) &&
            cacheItem.LastWriteTimeUtc == lastWriteUtc.Value)
        {
            file.CRC32 = cacheItem.CRC32;
            file.MD5 = cacheItem.MD5;
            file.SHA1 = cacheItem.SHA1;
            file.SHA256 = cacheItem.SHA256;
            return;
        }

        await this.inner.CalculateChecksumsAsync(file, ct);

        if (lastWriteUtc is not null)
        {
            this.cache.AddOrUpdate(
                cacheKey,
                new CacheItem(file, lastWriteUtc.Value),
                (_, existing) => new CacheItem(file, lastWriteUtc.Value));
        }

        static string MakeCacheKey(FileInformation file) => $"{file.ContainerAbsolutePath}+{file.FileRelativePath}";

        static DateTime? GetLastWriteUtc(FileInformation file)
        {
            DateTime? lastWriteUtc = null;

            if (file.ContainerIsFolder && Directory.Exists(file.ContainerAbsolutePath))
            {
                var fileAbsolutePath = Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath);
                if (File.Exists(fileAbsolutePath))
                {
                    lastWriteUtc = File.GetLastWriteTimeUtc(fileAbsolutePath);
                }
            }
            else if (!file.ContainerIsFolder && File.Exists(file.ContainerAbsolutePath))
            {
                // Archive file last write time is updated whenever its content changes.
                lastWriteUtc = File.GetLastWriteTimeUtc(file.ContainerAbsolutePath);
            }

            return lastWriteUtc;
        }
    }

    public Task CopyAsync(FileInformation file, string targetFilePath, CancellationToken ct)
    {
        return this.inner.CopyAsync(file, targetFilePath, ct);
    }

    public Task<bool> ExistsAsync(FileInformation file, CancellationToken ct)
    {
        return this.inner.ExistsAsync(file, ct);
    }

    public Task<FileInformation[]> GetAllFilesAsync(string containerFilePath, SearchOption searchOption, CancellationToken ct)
    {
        return this.inner.GetAllFilesAsync(containerFilePath, searchOption, ct);
    }

    public Task MoveAsync(FileInformation file, string targetFilePath, CancellationToken ct)
    {
        return this.inner.MoveAsync(file, targetFilePath, ct);
    }

    public Task RemoveAsync(FileInformation file, CancellationToken ct)
    {
        return this.inner.RemoveAsync(file, ct);
    }

    public Task RemoveContainerAsync(string containerFilePath, CancellationToken ct)
    {
        return this.inner.RemoveContainerAsync(containerFilePath, ct);
    }

    private class CacheItem
    {
        public CacheItem(FileInformation file, DateTime lastWriteUtc)
        {
            this.CRC32 = file.CRC32;
            this.MD5 = file.MD5;
            this.SHA1 = file.SHA1;
            this.SHA256 = file.SHA256;
            this.LastWriteTimeUtc = lastWriteUtc;
        }

        public byte[] CRC32 { get; }

        public byte[] MD5 { get; }

        public byte[] SHA1 { get; }

        public byte[] SHA256 { get; }

        public DateTime LastWriteTimeUtc { get; }
    }
}
