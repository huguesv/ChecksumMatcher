// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.UnitTest;

using Woohoo.IO.AbstractFileSystem.Online;
using Woohoo.IO.AbstractFileSystem.UnitTest.Infrastructure;

public partial class OnlineArchiveUnitTest
{
    [Theory]
    [InlineData("single.7z", true)]
    [InlineData("single.zip", true)]
    [InlineData("data1.txt", false)]
    public void IsSupportedArchiveFile(string archiveName, bool expected)
    {
        // Arrange
        var archive = new OnlineArchive();
        using var tempFolder = new DisposableTempFolder();
        var archivePath = Path.Combine(tempFolder.Path, archiveName);
        File.WriteAllBytes(archivePath, GetDataResourceBytes(archiveName));

        // Act
        var actual = archive.IsSupportedArchiveFile(archivePath);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("single.7z")]
    [InlineData("single.zip")]
    public void GetEntries_Single(string archiveName)
    {
        // Arrange
        var archive = new OnlineArchive();
        using var tempFolder = new DisposableTempFolder();
        var archivePath = Path.Combine(tempFolder.Path, archiveName);
        File.WriteAllBytes(archivePath, GetDataResourceBytes(archiveName));

        // Act
        var actual = archive.GetEntries(archivePath);

        // Assert
        actual.Should().ContainSingle();

        var data1Entry = actual.Single(e => e.Name == "data1.txt");
        data1Entry.Name.Should().Be("data1.txt");
        data1Entry.Size.Should().Be(3105);
        data1Entry.IsDirectory.Should().BeFalse();
        data1Entry.ReportedCRC32.Should().NotBeNull();
        ByteArrayUtility.ByteArrayToHex(data1Entry.ReportedCRC32).Should().Be("bb60311b");
    }

    [Theory]
    [InlineData("double.7z")]
    [InlineData("double.zip")]
    public void GetEntries_Double(string archiveName)
    {
        // Arrange
        var archive = new OnlineArchive();
        using var tempFolder = new DisposableTempFolder();
        var archivePath = Path.Combine(tempFolder.Path, archiveName);
        File.WriteAllBytes(archivePath, GetDataResourceBytes(archiveName));

        // Act
        var actual = archive.GetEntries(archivePath);

        // Assert
        actual.Should().HaveCount(2);

        var data1Entry = actual.Single(e => e.Name == "data1.txt");
        data1Entry.Name.Should().Be("data1.txt");
        data1Entry.Size.Should().Be(3105);
        data1Entry.IsDirectory.Should().BeFalse();
        data1Entry.ReportedCRC32.Should().NotBeNull();
        ByteArrayUtility.ByteArrayToHex(data1Entry.ReportedCRC32).Should().Be("bb60311b");

        var data2Entry = actual.Single(e => e.Name == "data2.txt");
        data2Entry.Name.Should().Be("data2.txt");
        data2Entry.Size.Should().Be(4426);
        data2Entry.IsDirectory.Should().BeFalse();
        data2Entry.ReportedCRC32.Should().NotBeNull();
        ByteArrayUtility.ByteArrayToHex(data2Entry.ReportedCRC32).Should().Be("49e1712a");
    }

    [Theory]
    [InlineData("folders.7z")]
    [InlineData("folders.zip")]
    public void GetEntries_Folders(string archiveName)
    {
        // Arrange
        var archive = new OnlineArchive();
        using var tempFolder = new DisposableTempFolder();
        var archivePath = Path.Combine(tempFolder.Path, archiveName);
        File.WriteAllBytes(archivePath, GetDataResourceBytes(archiveName));

        // Act
        var actual = archive.GetEntries(archivePath);

        // Assert
        actual.Should().HaveCount(5);

        var data1Entry = actual.Single(e => e.Name == @"folder1\data1.txt");
        data1Entry.Name.Should().Be(@"folder1\data1.txt");
        data1Entry.Size.Should().Be(3105);
        data1Entry.IsDirectory.Should().BeFalse();
        data1Entry.ReportedCRC32.Should().NotBeNull();
        ByteArrayUtility.ByteArrayToHex(data1Entry.ReportedCRC32).Should().Be("bb60311b");

        var data2Entry = actual.Single(e => e.Name == @"folder2\data2.txt");
        data2Entry.Name.Should().Be(@"folder2\data2.txt");
        data2Entry.Size.Should().Be(4426);
        data2Entry.IsDirectory.Should().BeFalse();
        data2Entry.ReportedCRC32.Should().NotBeNull();
        ByteArrayUtility.ByteArrayToHex(data2Entry.ReportedCRC32).Should().Be("49e1712a");

        foreach (var expectedName in new[] { @"folder1", @"folder2", @"folder3" })
        {
            var entry = actual.Single(e => e.Name == expectedName);
            entry.Name.Should().Be(expectedName);
            entry.Size.Should().Be(0);
            entry.IsDirectory.Should().BeTrue();
            entry.ReportedCRC32.Should().BeNull();
        }
    }

    [Theory]
    [InlineData("tree.7z")]
    [InlineData("tree.zip")]
    public void GetEntries_Tree(string archiveName)
    {
        // Arrange
        var archive = new OnlineArchive();
        using var tempFolder = new DisposableTempFolder();
        var archivePath = Path.Combine(tempFolder.Path, archiveName);
        File.WriteAllBytes(archivePath, GetDataResourceBytes(archiveName));

        // Act
        var actual = archive.GetEntries(archivePath);

        // Assert
        actual.Should().HaveCount(6);

        var data1Entry = actual.Single(e => e.Name == @"tree\sub1\subsub\data1.txt");
        data1Entry.Name.Should().Be(@"tree\sub1\subsub\data1.txt");
        data1Entry.Size.Should().Be(3105);
        data1Entry.IsDirectory.Should().BeFalse();
        data1Entry.ReportedCRC32.Should().NotBeNull();
        ByteArrayUtility.ByteArrayToHex(data1Entry.ReportedCRC32).Should().Be("bb60311b");

        var data2Entry = actual.Single(e => e.Name == @"tree\sub2\data2.txt");
        data2Entry.Name.Should().Be(@"tree\sub2\data2.txt");
        data2Entry.Size.Should().Be(4426);
        data2Entry.IsDirectory.Should().BeFalse();
        data2Entry.ReportedCRC32.Should().NotBeNull();
        ByteArrayUtility.ByteArrayToHex(data2Entry.ReportedCRC32).Should().Be("49e1712a");

        foreach (var expectedName in new[] { @"tree\sub1", @"tree\sub1\subsub", @"tree\sub2", "tree" })
        {
            var entry = actual.Single(e => e.Name == expectedName);
            entry.Name.Should().Be(expectedName);
            entry.Size.Should().Be(0);
            entry.IsDirectory.Should().BeTrue();
            entry.ReportedCRC32.Should().BeNull();
        }
    }

    private static byte[] GetDataResourceBytes(string resourceName)
    {
        return resourceName switch
        {
            "data1.txt" => DataResources.data1txt,
            "data2.txt" => DataResources.data2txt,
            "single.7z" => DataResources.single7z,
            "single.zip" => DataResources.singlezip,
            "double.7z" => DataResources.double7z,
            "double.zip" => DataResources.doublezip,
            "folders.7z" => DataResources.folders7z,
            "folders.zip" => DataResources.folderszip,
            "tree.7z" => DataResources.tree7z,
            "tree.zip" => DataResources.treezip,
            _ => throw new ArgumentException($"Unknown resource: {resourceName}", nameof(resourceName)),
        };
    }
}
