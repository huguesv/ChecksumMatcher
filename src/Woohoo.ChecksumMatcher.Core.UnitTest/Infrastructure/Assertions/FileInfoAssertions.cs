// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.UnitTest.Infrastructure.Assertions;

using System.IO.Compression;
using AwesomeAssertions.Execution;
using AwesomeAssertions.Primitives;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.IO.Compression.SevenZip;

internal class FileInfoAssertions : ReferenceTypeAssertions<FileInfo, FileInfoAssertions>
{
    private readonly AssertionChain chain;

    public FileInfoAssertions(FileInfo instance, AssertionChain chain)
        : base(instance, chain)
    {
        this.chain = chain;
    }

    protected override string Identifier => "file";

    [CustomAssertion]
    public AndConstraint<FileInfoAssertions> BeContainerType(string container, string because = "", params object[] becauseArgs)
    {
        this.chain
            .BecauseOf(because, becauseArgs)
            .Given(() => GetContainerType(this.Subject.FullName))
            .ForCondition(s => container == s)
            .FailWith("Expected {context:file} to be {0}{reason}, but it is {1}.", _ => container, c => c);

        return new AndConstraint<FileInfoAssertions>(this);
    }

    [CustomAssertion]
    public AndConstraint<FileInfoAssertions> HaveArchiveEntriesEquivalentTo(IEnumerable<string> entries, string because = "", params object[] becauseArgs)
    {
        this.chain
            .BecauseOf(because, becauseArgs)
            .Given(() => GetArchiveEntries(this.Subject.FullName))
            .ForCondition(s => s.Order().SequenceEqual(entries.Order()))
            .FailWith("Expected {context:file} to contain [{0}]{reason}, but found [{1}].", _ => string.Join(',', entries), actual => string.Join(',', actual));

        return new AndConstraint<FileInfoAssertions>(this);
    }

    [CustomAssertion]
    public AndConstraint<FileInfoAssertions> UseCompressionMethod(string method, string because = "", params object[] becauseArgs)
    {
        this.chain
            .BecauseOf(because, becauseArgs)
            .Given(() => GetSevenZipEntriesCompressionMethods(this.Subject.FullName))
            .ForCondition(s => s.All(cm => cm == method))
            .FailWith("Expected {context:file} to use compression method {0}{reason}, but found [{1}].", _ => method, actual => string.Join(',', new HashSet<string>(actual)));

        return new AndConstraint<FileInfoAssertions>(this);
    }

    [CustomAssertion]
    public AndConstraint<FileInfoAssertions> UseCompressionMethodPrefix(string methodPrefix, string because = "", params object[] becauseArgs)
    {
        this.chain
            .BecauseOf(because, becauseArgs)
            .Given(() => GetSevenZipEntriesCompressionMethods(this.Subject.FullName))
            .ForCondition(s => s.All(cm => cm.StartsWith(methodPrefix, StringComparison.InvariantCulture)))
            .FailWith("Expected {context:file} to use compression method prefix {0}{reason}, but found [{1}].", _ => methodPrefix, actual => string.Join(',', new HashSet<string>(actual)));

        return new AndConstraint<FileInfoAssertions>(this);
    }

    private static string[] GetSevenZipEntriesCompressionMethods(string filePath)
    {
        var archive = new SevenZipFile(filePath);
        return [.. archive.Entries.Select(e => e.Method)];
    }

    private static string[] GetArchiveEntries(string filePath)
    {
        if (string.Equals(Path.GetExtension(filePath), ".zip", StringComparison.OrdinalIgnoreCase))
        {
            return GetZipEntries(filePath);
        }
        else if (string.Equals(Path.GetExtension(filePath), ".7z", StringComparison.OrdinalIgnoreCase))
        {
            return GetSevenZipEntries(filePath);
        }
        else
        {
            throw new NotSupportedException($"Unsupported archive type for file: {filePath}");
        }
    }

    private static string[] GetZipEntries(string filePath)
    {
        using var archive = ZipFile.OpenRead(filePath);
        return [.. archive.Entries.Select(e => e.Name)];
    }

    private static string[] GetSevenZipEntries(string filePath)
    {
        var archive = new SevenZipFile(filePath);
        return [.. archive.Entries.Select(e => e.Name)];
    }

    private static string GetContainerType(string filePath)
    {
        if (IsTorrentZip(filePath))
        {
            return KnownContainerTypes.TorrentZip;
        }
        else if (IsTorrentSevenZip(filePath))
        {
            return KnownContainerTypes.TorrentSevenZip;
        }
        else if (string.Equals(Path.GetExtension(filePath), ".zip", StringComparison.OrdinalIgnoreCase))
        {
            return KnownContainerTypes.Zip;
        }
        else if (string.Equals(Path.GetExtension(filePath), ".7z", StringComparison.OrdinalIgnoreCase))
        {
            return KnownContainerTypes.SevenZip;
        }

        return string.Empty;
    }

    private static bool IsTorrentZip(string filePath)
    {
        if (!string.Equals(Path.GetExtension(filePath), ".zip", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return HasTorrentZipFooter(filePath);
    }

    private static bool IsTorrentSevenZip(string filePath)
    {
        if (!string.Equals(Path.GetExtension(filePath), ".7z", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return HasTorrentSevenZipFooter(filePath);
    }

    private static bool HasTorrentZipFooter(string filePath)
    {
        // File should end with "TORRENTZIPPED-XXXXXXXX" where Xs represent a CRC32
        const string marker = "TORRENTZIPPED-";
        var markerBytes = Encoding.ASCII.GetBytes(marker);
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        stream.Seek(0 - (marker.Length + 8), SeekOrigin.End);
        var buffer = new byte[marker.Length];
        var bytesRead = stream.Read(buffer, 0, marker.Length);

        return bytesRead == marker.Length && buffer.SequenceEqual(markerBytes);
    }

    private static bool HasTorrentSevenZipFooter(string filePath)
    {
        // File should end with "torrent7z_0.9beta"
        const string marker = "torrent7z_0.9beta";
        var markerBytes = Encoding.ASCII.GetBytes(marker);
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        stream.Seek(0 - marker.Length, SeekOrigin.End);
        var buffer = new byte[marker.Length];
        var bytesRead = stream.Read(buffer, 0, marker.Length);

        return bytesRead == marker.Length && buffer.SequenceEqual(markerBytes);
    }
}
