// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest;

using Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public class HashCalculatorViewModelUnitTest
{
    [UIFact]
    public void Create()
    {
        // Arrange

        // Act
        _ = new HashCalculatorViewModel(
            new TestClipboardService(),
            new TestFilePickerService(),
            new TestFileExplorerService(),
            new TestDispatcherQueueService(),
            new TestLogger<HashCalculatorViewModel>());

        // Assert
    }

    [UIFact]
    public async Task SelectFile()
    {
        // Arrange
        using var tempFile = DisposableTempFile.WithBytes(new byte[256]);

        var sut = new HashCalculatorViewModel(
            new TestClipboardService(),
            new FilePickerServiceBuilder().WithPath(tempFile.FilePath).Build(),
            new TestFileExplorerService(),
            new TestDispatcherQueueService(),
            new TestLogger<HashCalculatorViewModel>());

        // Act
        await sut.SelectFileCommand.ExecuteAsync(null);

        // Assert
        sut.Results.Should().ContainSingle();
        sut.Results[0].IsCalculating.Should().BeFalse();
        sut.Results[0].IsCalculatingError.Should().BeFalse();
        sut.Results[0].FileProgress.Should().Be(100);
        sut.Results[0].FullPath.Should().Be(tempFile.FilePath);
        sut.Results[0].FolderPath.Should().Be(tempFile.FolderPath);
        sut.Results[0].FileSize.Should().Be(256);
        sut.Results[0].Crc32.Should().Be("0d968558");
        sut.Results[0].Md5.Should().Be("348a9791dc41b89796ec3808b5b5262f");
        sut.Results[0].Sha1.Should().Be("b376885ac8452b6cbf9ced81b1080bfd570d9b91");
        sut.Results[0].Sha256.Should().Be("5341e6b2646979a70e57653007a1f310169421ec9bdd9f1a5648f75ade005af1");
    }

    [UIFact]
    public async Task Clear()
    {
        // Arrange
        using var tempFile1 = DisposableTempFile.WithBytes(new byte[256]);
        using var tempFile2 = DisposableTempFile.WithBytes(new byte[512]);

        var sut = new HashCalculatorViewModel(
            new TestClipboardService(),
            new FilePickerServiceBuilder()
                .WithPath(tempFile1.FilePath)
                .WithPath(tempFile2.FilePath)
                .Build(),
            new TestFileExplorerService(),
            new TestDispatcherQueueService(),
            new TestLogger<HashCalculatorViewModel>());

        await sut.SelectFileCommand.ExecuteAsync(null);
        await sut.SelectFileCommand.ExecuteAsync(null);

        // Act
        sut.ClearCommand.Execute(null);

        // Assert
        sut.Results.Should().BeEmpty();
    }

    [UIFact]
    public async Task CopyToClipboard()
    {
        // Arrange
        using var tempFile = DisposableTempFile.WithBytes(new byte[256]);

        var clipboardService = new TestClipboardService();
        var sut = new HashCalculatorViewModel(
            clipboardService,
            new FilePickerServiceBuilder().WithPath(tempFile.FilePath).Build(),
            new TestFileExplorerService(),
            new TestDispatcherQueueService(),
            new TestLogger<HashCalculatorViewModel>());

        await sut.SelectFileCommand.ExecuteAsync(null);

        // Act
        sut.Results[0].CopyHashCommand.Execute(null);

        // Assert
        var expected = $"""
File: {tempFile.FileName}
Size: 256
CRC32: 0d968558
MD5: 348a9791dc41b89796ec3808b5b5262f
SHA1: b376885ac8452b6cbf9ced81b1080bfd570d9b91
SHA256: 5341e6b2646979a70e57653007a1f310169421ec9bdd9f1a5648f75ade005af1

""";
        clipboardService.CurrentText.Should().Be(expected);
    }

    [UIFact]
    public async Task OpenInExplorer()
    {
        // Arrange
        using var tempFile = DisposableTempFile.WithBytes(new byte[256]);

        var fileExplorerService = new TestFileExplorerService();
        var sut = new HashCalculatorViewModel(
            new TestClipboardService(),
            new FilePickerServiceBuilder().WithPath(tempFile.FilePath).Build(),
            fileExplorerService,
            new TestDispatcherQueueService(),
            new TestLogger<HashCalculatorViewModel>());

        await sut.SelectFileCommand.ExecuteAsync(null);

        // Act
        sut.Results[0].OpenInExplorerCommand.Execute(null);

        // Assert
        fileExplorerService.CurrentFilePath.Should().Be(tempFile.FilePath);
    }
}
