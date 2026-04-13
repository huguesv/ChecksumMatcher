// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.UnitTest;

using Woohoo.ChecksumMatcher.Core.Services;
using Woohoo.ChecksumMatcher.Core.UnitTest.Infrastructure;

public class RedumpArtifactsServiceUnitTest
{
    [Fact]
    public void CleanupContents_NoFilesInFolder()
    {
        // Arrange
        using var folder = new DisposableTempFolder();
        var sut = new RedumpArtifactsService();

        // Act
        var deletedFiles = sut.CleanupContents(folder.Path);

        // Assert
        deletedFiles.Should().BeEmpty();
    }

    [Fact]
    public void CleanupContents_NoArtifactsInFolder()
    {
        // Arrange
        using var folder = new DisposableTempFolder();
        var file1 = Path.Combine(folder.Path, "MySystem (123) (2024-01-01 12-00-00).txt");
        var file2 = Path.Combine(folder.Path, "MySystem (123) (2024-01-02).zip");
        var file3 = Path.Combine(folder.Path, "MySystem (123) (2024-01-03).zip");
        File.WriteAllText(file1, string.Empty);
        File.WriteAllText(file2, string.Empty);
        File.WriteAllText(file3, string.Empty);
        var sut = new RedumpArtifactsService();

        // Act
        var deletedFiles = sut.CleanupContents(folder.Path);

        // Assert
        deletedFiles.Should().BeEmpty();
        File.Exists(file1).Should().BeTrue();
        File.Exists(file2).Should().BeTrue();
        File.Exists(file3).Should().BeTrue();
    }

    [Fact]
    public void CleanupContents_MultipleArtifactsInFolder()
    {
        // Arrange
        using var folder = new DisposableTempFolder();
        var file1 = Path.Combine(folder.Path, "MySystem (123) (2024-01-01 12-00-00).zip");
        var file2 = Path.Combine(folder.Path, "MySystem (123) (2024-01-02 12-00-00).zip");
        var file3 = Path.Combine(folder.Path, "MySystem (123) (2024-01-03 12-00-00).zip");
        var file4 = Path.Combine(folder.Path, "MyOtherSystem (123) (2024-04-10 12-00-00).zip");
        File.WriteAllText(file1, string.Empty);
        File.WriteAllText(file2, string.Empty);
        File.WriteAllText(file3, string.Empty);
        File.WriteAllText(file4, string.Empty);
        File.SetAttributes(file1, FileAttributes.ReadOnly);
        var sut = new RedumpArtifactsService();

        // Act
        var deletedFiles = sut.CleanupContents(folder.Path);

        // Assert
        deletedFiles.Should().HaveCount(2);
        deletedFiles.Should().Contain(file1);
        deletedFiles.Should().Contain(file2);
        File.Exists(file1).Should().BeFalse();
        File.Exists(file2).Should().BeFalse();
        File.Exists(file3).Should().BeTrue();
        File.Exists(file4).Should().BeTrue();
    }

    [Fact]
    public void DeleteContents()
    {
        // Arrange
        var folder = new DisposableTempFolder();
        Directory.CreateDirectory(Path.Combine(folder.Path, "SubFolder"));
        var file1 = Path.Combine(folder.Path, "MySystem (123) (2024-01-01 12-00-00).zip");
        var file2 = Path.Combine(folder.Path, "MySystem (123) (2024-01-02 12-00-00).zip");
        var file3 = Path.Combine(folder.Path, "SubFolder", "MySubSystem (123) (2024-01-01 12-00-00).zip");
        File.WriteAllText(file1, string.Empty);
        File.WriteAllText(file2, string.Empty);
        File.WriteAllText(file3, string.Empty);
        File.SetAttributes(file1, FileAttributes.ReadOnly);
        var sut = new RedumpArtifactsService();

        // Act
        sut.DeleteContents(folder.Path);

        // Assert
        Directory.EnumerateFiles(folder.Path).Should().BeEmpty();
        Directory.EnumerateDirectories(folder.Path).Should().BeEmpty();
    }
}
