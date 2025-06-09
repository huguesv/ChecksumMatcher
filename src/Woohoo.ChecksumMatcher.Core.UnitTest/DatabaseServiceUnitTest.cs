// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.UnitTest;

using System;
using System.Threading;
using System.Threading.Tasks;
using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Services;
using Woohoo.ChecksumMatcher.Core.UnitTest.Infrastructure;
using Woohoo.ChecksumMatcher.Core.UnitTest.Infrastructure.Assertions;
using Woohoo.IO.AbstractFileSystem.Offline.Models;
using Xunit.Internal;

public class DatabaseServiceUnitTest
{
    [Fact]
    public async Task GetRepository_FoldersNotSet()
    {
        // Arrange
        var sut = new DatabaseService(new TestLogger<DatabaseService>(), new TestLocalSettingsService(), new TestOfflineExplorerService());

        // Act
        var actual = await sut.GetRepositoryAsync(CancellationToken.None);

        // Assert
        actual.RootFolders.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRepositoryFolders()
    {
        // Arrange
        using var databasesFolder1 = new DisposableTempFolder();
        using var databasesFolder2 = new DisposableTempFolder();

        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = new Dictionary<string, object?>
            {
                { KnownSettingKeys.DatabaseFolders, new string[] { databasesFolder1.Path, databasesFolder2.Path } },
            },
        };

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, new TestOfflineExplorerService());

        // Act
        var actual = await sut.GetRepositoryFoldersAsync(CancellationToken.None);

        // Assert
        actual.Should().BeEquivalentTo([databasesFolder1.Path, databasesFolder2.Path]);
    }

    [Fact]
    public async Task AddRepositoryFolder()
    {
        // Arrange
        using var databasesFolder1 = new DisposableTempFolder();
        using var databasesFolder2 = new DisposableTempFolder();

        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = new Dictionary<string, object?>
            {
                { KnownSettingKeys.DatabaseFolders, new string[] { databasesFolder1.Path } },
            },
        };

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, new TestOfflineExplorerService());

        // Act
        await sut.AddRepositoryFolderAsync(databasesFolder2.Path, CancellationToken.None);

        // Assert
        var actual = await sut.GetRepositoryFoldersAsync(CancellationToken.None);
        actual.Should().BeEquivalentTo([databasesFolder1.Path, databasesFolder2.Path]);
    }

    [Fact]
    public async Task RemoveRepositoryFolder()
    {
        // Arrange
        using var databasesFolder1 = new DisposableTempFolder();
        using var databasesFolder2 = new DisposableTempFolder();
        using var databasesFolder3 = new DisposableTempFolder();

        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = new Dictionary<string, object?>
            {
                { KnownSettingKeys.DatabaseFolders, new string[] { databasesFolder1.Path, databasesFolder2.Path, databasesFolder3.Path } },
            },
        };

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, new TestOfflineExplorerService());

        // Act
        await sut.RemoveRepositoryFolderAsync(databasesFolder1.Path, CancellationToken.None);

        // Assert
        var actual = await sut.GetRepositoryFoldersAsync(CancellationToken.None);
        actual.Should().BeEquivalentTo([databasesFolder2.Path, databasesFolder3.Path]);
    }

    [Theory]
    [InlineData(KnownContainerTypes.Folder, true, true, false)]
    [InlineData(KnownContainerTypes.Folder, true, false, false)]
    [InlineData(KnownContainerTypes.Folder, false, true, false)]
    [InlineData(KnownContainerTypes.Folder, false, false, false)]
    [InlineData(KnownContainerTypes.Zip, true, true, true)]
    [InlineData(KnownContainerTypes.Zip, true, true, false)]
    [InlineData(KnownContainerTypes.Zip, true, false, true)]
    [InlineData(KnownContainerTypes.Zip, true, false, false)]
    [InlineData(KnownContainerTypes.Zip, false, true, true)]
    [InlineData(KnownContainerTypes.Zip, false, true, false)]
    [InlineData(KnownContainerTypes.Zip, false, false, true)]
    [InlineData(KnownContainerTypes.Zip, false, false, false)]
    [InlineData(KnownContainerTypes.SevenZip, true, true, true)]
    [InlineData(KnownContainerTypes.SevenZip, true, true, false)]
    [InlineData(KnownContainerTypes.SevenZip, true, false, true)]
    [InlineData(KnownContainerTypes.SevenZip, true, false, false)]
    [InlineData(KnownContainerTypes.SevenZip, false, true, true)]
    [InlineData(KnownContainerTypes.SevenZip, false, true, false)]
    [InlineData(KnownContainerTypes.SevenZip, false, false, true)]
    [InlineData(KnownContainerTypes.SevenZip, false, false, false)]
    [InlineData(KnownContainerTypes.TorrentZip, true, true, true)]
    [InlineData(KnownContainerTypes.TorrentZip, true, true, false)]
    [InlineData(KnownContainerTypes.TorrentZip, true, false, true)]
    [InlineData(KnownContainerTypes.TorrentZip, true, false, false)]
    [InlineData(KnownContainerTypes.TorrentZip, false, true, true)]
    [InlineData(KnownContainerTypes.TorrentZip, false, true, false)]
    [InlineData(KnownContainerTypes.TorrentZip, false, false, true)]
    [InlineData(KnownContainerTypes.TorrentZip, false, false, false)]
    [InlineData(KnownContainerTypes.TorrentSevenZip, true, true, true)]
    [InlineData(KnownContainerTypes.TorrentSevenZip, true, true, false)]
    [InlineData(KnownContainerTypes.TorrentSevenZip, true, false, true)]
    [InlineData(KnownContainerTypes.TorrentSevenZip, true, false, false)]
    [InlineData(KnownContainerTypes.TorrentSevenZip, false, true, true)]
    [InlineData(KnownContainerTypes.TorrentSevenZip, false, true, false)]
    [InlineData(KnownContainerTypes.TorrentSevenZip, false, false, true)]
    [InlineData(KnownContainerTypes.TorrentSevenZip, false, false, false)]
    public async Task Rebuild(string containerType, bool findMissingCueFiles, bool moveIncomplete, bool torrentZipIncomplete)
    {
        if (containerType == KnownContainerTypes.TorrentSevenZip)
        {
            // TODO: figure out why we get an error from t7z "unable to uncompress file" on GitHub Actions.
            Assert.SkipWhen(
                !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE")),
                "Skipping on GitHub Actions as it is not supported yet.");
        }

        // Arrange
        using var sourceFolder = new DisposableTempFolder();
        using var cueFolder = new DisposableTempFolder();
        using var databasesFolder = new DisposableTempFolder();
        using var rebuildTargetFolder = new DisposableTempFolder();
        using var rebuildIncompleteTargetFolder = new DisposableTempFolder();

        var sourceFile1 = new RandomFile(Path.Combine(sourceFolder.Path, "random600k.bin"), 600_000, 42);
        var sourceFile2a = new RandomFile(Path.Combine(sourceFolder.Path, "game2-file-a.bin"), 3_000, 42);
        var sourceFile2b = new RandomFile(Path.Combine(sourceFolder.Path, "game2-file-b.bin"), 1_000, 42);
        var sourceFile3 = new RandomFile(Path.Combine(sourceFolder.Path, "unused.bin"), 2_300, 42);
        var sourceFileGame4Cue = new RandomFile(Path.Combine(sourceFolder.Path, "game4.cue"), 100, 50);
        var sourceFileGame4Track1 = new RandomFile(Path.Combine(sourceFolder.Path, "game4track1.bin"), 2_300, 51);
        var sourceFileGame4Track2 = new RandomFile(Path.Combine(sourceFolder.Path, "game4track2.bin"), 2_300, 52);
        var cueFileGame5 = new RandomFile(Path.Combine(cueFolder.Path, "game 5.cue"), 100, 60);
        var sourceFileGame5Track1 = new RandomFile(Path.Combine(sourceFolder.Path, "game5track1.bin"), 2_300, 61);
        var sourceFileGame5Track2 = new RandomFile(Path.Combine(sourceFolder.Path, "game5track2.bin"), 2_300, 62);

        new DatabaseBuilder()
            .WithName("test")

            // Single file found in source folder
            .WithGame(g => g
                .WithName("game 1")
                .WithRomFile("file1.bin", sourceFile1))

            // All files found in source folder
            .WithGame(g => g
                .WithName("game 2")
                .WithRomFile("game2-a.bin", sourceFile2a)
                .WithRomFile("game2-b.bin", sourceFile2b))

            // Track 3 missing
            .WithGame(g => g
                .WithName("game 4")
                .WithRomFile("game 4.cue", sourceFileGame4Cue)
                .WithRomFile("game 4 (track 1).bin", sourceFileGame4Track1)
                .WithRomFile("game 4 (track 2).bin", sourceFileGame4Track2)
                .WithRomFile("game 4 (track 3).bin", 500, "bbff86e6", "43f8df1895dc415f1b69b52a3804a500", "738ed16508d3faec2885e65d58135a1d4b3c7500", "b2ca48cd022b7b2c9fe3290eb9542928c46322319725b9fb7877cc27996d3a24"))

            // All tracks found in source folder, cue file found in cue folder
            .WithGame(g => g
                .WithName("game 5")
                .WithRomFile("game 5.cue", cueFileGame5)
                .WithRomFile("game 5 (track 1).bin", sourceFileGame5Track1)
                .WithRomFile("game 5 (track 2).bin", sourceFileGame5Track2))

            .Build(Path.Combine(databasesFolder.Path, "db.dat"));

        var rebuildSettings = new RebuildSettings
        {
            FindMissingCueFiles = findMissingCueFiles,
            TorrentZipIncomplete = torrentZipIncomplete,
            ForceCalculateChecksums = false,
            RemoveSource = false,
            TargetContainerType = containerType,
            SourceFolderPath = sourceFolder.Path,
            TargetFolderPath = rebuildTargetFolder.Path,
            TargetIncompleteFolderPath = moveIncomplete ? rebuildIncompleteTargetFolder.Path : string.Empty,
        };

        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = new Dictionary<string, object?>
            {
                { KnownSettingKeys.CueFolders, new string[] { cueFolder.Path } },
                { KnownSettingKeys.DatabaseFolders, new string[] { databasesFolder.Path } },
                { KnownSettingKeys.RebuildSettings, new DatabaseScopeSetting<RebuildSettings> { Entries = { ["test"] = rebuildSettings } } },
            },
        };

        var offlineExplorerService = new TestOfflineExplorerService();

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, offlineExplorerService);

        int? progressPercentage = null;
        List<RebuildStatus> statuses = [];
        List<RomAndFileMoniker> matched = [];
        List<FileMoniker> unused = [];
        sut.RebuildProgress += (sender, args) =>
        {
            matched.AddRange(args.Results.Matched);
            unused.AddRange(args.Results.Unused);
            progressPercentage = args.ProgressPercentage;
            statuses.Add(args.Status);
        };

        var databaseFile = new DatabaseFile { RootAbsoluteFolderPath = databasesFolder.Path, RelativePath = "db.dat" };

        // Act
        await sut.RebuildAsync(databaseFile, CancellationToken.None);

        // Assert
        var incompleteContainerType = containerType switch
        {
            KnownContainerTypes.TorrentZip => torrentZipIncomplete ? containerType : KnownContainerTypes.Zip,
            KnownContainerTypes.TorrentSevenZip => torrentZipIncomplete ? containerType : KnownContainerTypes.Zip,
            _ => containerType,
        };

        RebuiltResult[] expectedMatches =
        [
            new("game 1", rebuildTargetFolder.Path, containerType, [new("file1.bin", sourceFile1)]),
            new("game 2", rebuildTargetFolder.Path, containerType, [new("game2-a.bin", sourceFile2a), new("game2-b.bin", sourceFile2b)]),
            new("game 4", rebuildTargetFolder.Path, incompleteContainerType, [new("game 4.cue", sourceFileGame4Cue), new("game 4 (track 1).bin", sourceFileGame4Track1), new("game 4 (track 2).bin", sourceFileGame4Track2)]),
            new("game 5", rebuildTargetFolder.Path, containerType, [new("game 5.cue", cueFileGame5), new("game 5 (track 1).bin", sourceFileGame5Track1), new("game 5 (track 2).bin", sourceFileGame5Track2)]),
        ];

        if (moveIncomplete)
        {
            expectedMatches[2].ParentFolder = rebuildIncompleteTargetFolder.Path;
        }

        if (!findMissingCueFiles)
        {
            expectedMatches[3].Entries.RemoveAt(0);
            expectedMatches[3].ContainerType = incompleteContainerType;
            if (moveIncomplete)
            {
                expectedMatches[3].ParentFolder = rebuildIncompleteTargetFolder.Path;
            }
        }

        if (containerType == KnownContainerTypes.TorrentSevenZip)
        {
            Directory
                .GetFiles(rebuildTargetFolder.Path, "*.log")
                .Select(f => $"--- START of {f}\n{File.ReadAllText(f)}\n--- END of {f}\n\n")
                .Should()
                .BeEmpty();
        }

        // Check that the target folders contain the expected rebuilt containers
        foreach (var targetFolderPath in new string[] { rebuildTargetFolder.Path, rebuildIncompleteTargetFolder.Path })
        {
            Directory
                .GetDirectories(targetFolderPath)
                .Select(f => Path.GetFileName(f))
                .Should()
                .BeEquivalentTo(expectedMatches
                    .Where(r => r.ParentFolder == targetFolderPath)
                    .Where(r => r.ContainerType == KnownContainerTypes.Folder)
                    .Select(r => r.NameWithExtension));

            Directory
                .GetFiles(targetFolderPath)
                .Select(f => Path.GetFileName(f))
                .Should()
                .BeEquivalentTo(expectedMatches
                    .Where(r => r.ParentFolder == targetFolderPath)
                    .Where(r => r.ContainerType != KnownContainerTypes.Folder)
                    .Select(r => r.NameWithExtension));
        }

        // Check the contents of every rebuilt container (folder)
        expectedMatches
            .Where(r => r.ContainerType == KnownContainerTypes.Folder)
            .Select(r => new { Result = r, DirectoryInfo = new DirectoryInfo(r.FullPath) })
            .ForEach(res => res.DirectoryInfo
                .GetFiles()
                .Select(fi => fi.Name)
                .Should()
                .BeEquivalentTo(res.Result.Entries.Select(e => e.Name)));

        // Check the contents of every rebuilt container (archive file)
        var files = expectedMatches
            .Where(r => r.ContainerType != KnownContainerTypes.Folder)
            .Select(r => new { Result = r, FileInfo = new FileInfo(r.FullPath) });

        files.ForEach(res => res.FileInfo.Should().HaveArchiveEntriesEquivalentTo(res.Result.Entries.Select(e => e.Name)));
        files.ForEach(res => res.FileInfo.Should().BeContainerType(res.Result.ContainerType));
        files
            .Where(res => res.Result.ContainerType == KnownContainerTypes.TorrentSevenZip)
            .ForEach(res => res.FileInfo.Should().UseCompressionMethodPrefix("LZMA"));
        files
            .Where(res => res.Result.ContainerType == KnownContainerTypes.SevenZip)
            .ForEach(res => res.FileInfo.Should().UseCompressionMethod("Copy"));

        matched.Should().BeEquivalentTo(
            expectedMatches.SelectMany(r => r.Entries.Select(e => new RomAndFileMoniker(new(r.Name, e.Name), new(e.Source.FolderPath, e.Source.FolderName, e.Source.FileName, false)))));

        unused.Should().BeEquivalentTo([
            new FileMoniker(sourceFile3.FolderPath, sourceFile3.FolderName, sourceFile3.FileName, false)
        ]);

        progressPercentage.Should().Be(100);
        statuses.Take(2).Should().BeEquivalentTo([
            RebuildStatus.Pending,
            RebuildStatus.Started,
        ]);
        statuses.Last().Should().Be(RebuildStatus.Completed);
    }

    [Fact]
    public async Task Scan_DatabaseFile()
    {
        // Arrange
        using var scanFolder = new DisposableTempFolder();
        var sourceFile1 = new RandomFile(Path.Combine(scanFolder.Path, "random600k.bin"), 600_000, 42);
        var sourceFile2a = new RandomFile(Path.Combine(scanFolder.Path, "game2", "game2-a.bin"), 3_000, 42);
        var sourceFile2b = new RandomFile(Path.Combine(scanFolder.Path, "game2", "game2-b.bin"), 1_000, 42);
        var sourceFile3 = new RandomFile(Path.Combine(scanFolder.Path, "unused.bin"), 2_000, 42);

        var dbFileScanSettings = new DatabaseFileScanSettings
        {
            UseOnlineFolders = true,
            ScanOnlineFolders =
            [
                new()
                {
                    FolderPath = scanFolder.Path,
                    IsIncluded = true,
                },
            ],
        };

        using var databasesFolder = new DisposableTempFolder();
        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = new Dictionary<string, object?>
            {
                { KnownSettingKeys.DatabaseFolders, new string[] { databasesFolder.Path } },
                { KnownSettingKeys.DatabaseFileScanSettings, new DatabaseScopeSetting<DatabaseFileScanSettings> { Entries = { ["test"] = dbFileScanSettings } } },
            },
        };
        new DatabaseBuilder()
            .WithName("test")
            .WithGame(g => g
                .WithName("game1")
                .WithRomFile("file1.bin", sourceFile1))
            .WithGame(g => g
                .WithName("game2")
                .WithRomFile("game2-a.bin", sourceFile2a)
                .WithRomFile("game2-b.bin", sourceFile2b))
            .WithGame(g => g
                .WithName("game3")
                .WithRomFile("file3.bin", 500, "bbff86e6", "43f8df1895dc415f1b69b52a3804a500", "738ed16508d3faec2885e65d58135a1d4b3c7500", "b2ca48cd022b7b2c9fe3290eb9542928c46322319725b9fb7877cc27996d3a24"))
            .Build(Path.Combine(databasesFolder.Path, "db.dat"));

        var offlineExplorerService = new TestOfflineExplorerService();

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, offlineExplorerService);

        int? progressPercentage = null;
        List<ScanStatus> statuses = [];
        List<RomAndFileMoniker> matched = [];
        List<RomAndFileMoniker> wrongNamed = [];
        List<FileMoniker> unused = [];
        List<RomMoniker> missing = [];
        sut.ScanProgress += (sender, args) =>
        {
            matched.AddRange(args.Results.Matched);
            wrongNamed.AddRange(args.Results.WrongNamed);
            unused.AddRange(args.Results.Unused);
            missing.AddRange(args.Results.Missing);
            progressPercentage = args.ProgressPercentage;
            statuses.Add(args.Status);
        };

        var databaseFile = new DatabaseFile { RootAbsoluteFolderPath = databasesFolder.Path, RelativePath = "db.dat" };

        // Act
        await sut.ScanAsync(databaseFile, CancellationToken.None);

        // Assert
        wrongNamed.Should().BeEquivalentTo([
            new RomAndFileMoniker(
                new RomMoniker("game1", "file1.bin"),
                new FileMoniker(sourceFile1.FolderPath, sourceFile1.FolderName, sourceFile1.FileName, false))
        ]);
        matched.Should().BeEquivalentTo([
            new RomAndFileMoniker(
                new RomMoniker("game2", "game2-a.bin"),
                new FileMoniker(sourceFile2a.FolderPath, sourceFile2a.FolderName, sourceFile2a.FileName, false)),
            new RomAndFileMoniker(
                new RomMoniker("game2", "game2-b.bin"),
                new FileMoniker(sourceFile2b.FolderPath, sourceFile2b.FolderName, sourceFile2b.FileName, false)),
        ]);
        unused.Should().BeEquivalentTo([
            new FileMoniker(sourceFile3.FolderPath, sourceFile3.FolderName, sourceFile3.FileName, false)
        ]);
        missing.Should().BeEquivalentTo([
            new RomMoniker("game3", "file3.bin")
        ]);

        progressPercentage.Should().Be(100);
        statuses.Take(2).Should().BeEquivalentTo([
            ScanStatus.Pending,
            ScanStatus.Started,
        ]);
        statuses.Last().Should().Be(ScanStatus.Completed);
    }

    [Fact]
    public async Task Scan_DatabaseFolder()
    {
        // Arrange
        using var scanFolder = new DisposableTempFolder();
        var sourceFile1 = new RandomFile(Path.Combine(scanFolder.Path, "testdb1", "random600k.bin"), 600_000, 42);
        var sourceFile2a = new RandomFile(Path.Combine(scanFolder.Path, "testdb1", "game2", "game2-a.bin"), 3_000, 42);
        var sourceFile2b = new RandomFile(Path.Combine(scanFolder.Path, "testdb1", "game2", "game2-b.bin"), 1_000, 42);
        var sourceFile3 = new RandomFile(Path.Combine(scanFolder.Path, "testdb1", "unused.bin"), 2_000, 42);
        var sourceFile4a = new RandomFile(Path.Combine(scanFolder.Path, "testdb2", "game4", "game4-a.bin"), 3_002, 42);
        var sourceFile4b = new RandomFile(Path.Combine(scanFolder.Path, "testdb2", "game4", "game4-b.bin"), 1_002, 42);

        var dbFolderScanSettings = new DatabaseFolderScanSettings
        {
            UseOnlineFolders = true,
            ScanOnlineFolders =
            [
                new()
                {
                    FolderPath = scanFolder.Path,
                    IsIncluded = true,
                },
            ],
        };

        using var databasesFolder = new DisposableTempFolder();
        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = new Dictionary<string, object?>
            {
                { KnownSettingKeys.DatabaseFolders, new string[] { databasesFolder.Path } },
                { KnownSettingKeys.DatabaseFolderScanSettings, new DatabaseScopeSetting<DatabaseFolderScanSettings> { Entries = { [databasesFolder.Path] = dbFolderScanSettings } } },
            },
        };
        new DatabaseBuilder()
            .WithName("testdb1")
            .WithGame(g => g
                .WithName("game1")
                .WithRomFile("file1.bin", sourceFile1))
            .WithGame(g => g
                .WithName("game2")
                .WithRomFile("game2-a.bin", sourceFile2a)
                .WithRomFile("game2-b.bin", sourceFile2b))
            .WithGame(g => g
                .WithName("game3")
                .WithRomFile("file3.bin", 500, "bbff86e6", "43f8df1895dc415f1b69b52a3804a500", "738ed16508d3faec2885e65d58135a1d4b3c7500", "b2ca48cd022b7b2c9fe3290eb9542928c46322319725b9fb7877cc27996d3a24"))
            .Build(Path.Combine(databasesFolder.Path, "db1.dat"));
        new DatabaseBuilder()
            .WithName("testdb2")
            .WithGame(g => g
                .WithName("game4")
                .WithRomFile("game4-a.bin", sourceFile4a)
                .WithRomFile("game4-b.bin", sourceFile4b))
            .Build(Path.Combine(databasesFolder.Path, "db2.dat"));

        var offlineExplorerService = new TestOfflineExplorerService();

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, offlineExplorerService);

        Dictionary<string, List<ScanStatus>> statuses = [];
        Dictionary<string, List<RomAndFileMoniker>> matched = [];
        Dictionary<string, List<RomAndFileMoniker>> wrongNamed = [];
        Dictionary<string, List<FileMoniker>> unused = [];
        Dictionary<string, List<RomMoniker>> missing = [];
        sut.ScanProgress += (sender, args) =>
        {
            if (matched.TryGetValue(args.DatabaseFile.FullPath, out var existingMatched))
            {
                existingMatched.AddRange(args.Results.Matched);
            }
            else
            {
                matched[args.DatabaseFile.FullPath] = [.. args.Results.Matched];
            }

            if (wrongNamed.TryGetValue(args.DatabaseFile.FullPath, out var existingWrongNamed))
            {
                existingWrongNamed.AddRange(args.Results.WrongNamed);
            }
            else
            {
                wrongNamed[args.DatabaseFile.FullPath] = [.. args.Results.WrongNamed];
            }

            if (unused.TryGetValue(args.DatabaseFile.FullPath, out var existingUnused))
            {
                existingUnused.AddRange(args.Results.Unused);
            }
            else
            {
                unused[args.DatabaseFile.FullPath] = [.. args.Results.Unused];
            }

            if (missing.TryGetValue(args.DatabaseFile.FullPath, out var existingMissing))
            {
                existingMissing.AddRange(args.Results.Missing);
            }
            else
            {
                missing[args.DatabaseFile.FullPath] = [.. args.Results.Missing];
            }
        };

        var databaseFile1 = new DatabaseFile { RootAbsoluteFolderPath = databasesFolder.Path, RelativePath = "db1.dat" };
        var databaseFile2 = new DatabaseFile { RootAbsoluteFolderPath = databasesFolder.Path, RelativePath = "db2.dat" };
        var databaseFolder = new DatabaseFolder
        {
            RootAbsoluteFolderPath = databasesFolder.Path,
            RelativePath = string.Empty,
            Files = [databaseFile1, databaseFile2],
            SubFolders = [],
        };

        // Act
        await sut.ScanAsync(databaseFolder, CancellationToken.None);

        // Assert
        wrongNamed[databaseFile1.FullPath].Should().BeEquivalentTo([
            new RomAndFileMoniker(
                new RomMoniker("game1", "file1.bin"),
                new FileMoniker(sourceFile1.FolderPath, sourceFile1.FolderName, sourceFile1.FileName, false))
        ]);
        matched[databaseFile1.FullPath].Should().BeEquivalentTo([
            new RomAndFileMoniker(
                new RomMoniker("game2", "game2-a.bin"),
                new FileMoniker(sourceFile2a.FolderPath, sourceFile2a.FolderName, sourceFile2a.FileName, false)),
            new RomAndFileMoniker(
                new RomMoniker("game2", "game2-b.bin"),
                new FileMoniker(sourceFile2b.FolderPath, sourceFile2b.FolderName, sourceFile2b.FileName, false)),
        ]);
        unused[databaseFile1.FullPath].Should().BeEquivalentTo([
            new FileMoniker(sourceFile3.FolderPath, sourceFile3.FolderName, sourceFile3.FileName, false)
        ]);
        missing[databaseFile1.FullPath].Should().BeEquivalentTo([
            new RomMoniker("game3", "file3.bin")
        ]);

        wrongNamed[databaseFile2.FullPath].Should().BeEmpty();
        matched[databaseFile2.FullPath].Should().BeEquivalentTo([
            new RomAndFileMoniker(
                new RomMoniker("game4", "game4-a.bin"),
                new FileMoniker(sourceFile4a.FolderPath, sourceFile4a.FolderName, sourceFile4a.FileName, false)),
            new RomAndFileMoniker(
                new RomMoniker("game4", "game4-b.bin"),
                new FileMoniker(sourceFile4b.FolderPath, sourceFile4b.FolderName, sourceFile4b.FileName, false)),
        ]);
        unused[databaseFile2.FullPath].Should().BeEmpty();
        missing[databaseFile2.FullPath].Should().BeEmpty();
    }

    [Fact]
    public async Task CreateDatabase()
    {
        // Arrange
        using var sourceFolder = new DisposableTempFolder();
        var sourceFile2a = new RandomFile(Path.Combine(sourceFolder.Path, "game2", "game2-a.bin"), 3_000, 42);
        var sourceFile2b = new RandomFile(Path.Combine(sourceFolder.Path, "game2", "game2-b.bin"), 1_000, 42);

        using var databasesFolder = new DisposableTempFolder();
        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = new Dictionary<string, object?>
            {
                { KnownSettingKeys.DatabaseFolders, new string[] { databasesFolder.Path } },
            },
        };

        var offlineExplorerService = new TestOfflineExplorerService();

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, offlineExplorerService);

        int? progressPercentage = null;
        List<DatabaseCreateStatus> statuses = [];
        List<RomAndFileMoniker> added = [];
        sut.DatabaseCreateProgress += (sender, args) =>
        {
            added.AddRange(args.Results.Added);
            progressPercentage = args.ProgressPercentage;
            statuses.Add(args.Status);
        };

        var databaseFilePath = Path.Combine(databasesFolder.Path, "db.dat");
        var databaseCreateSettings = new DatabaseCreateSettings()
        {
            Name = "testdb",
            Author = "author",
            Description = "description",
            Category = "category",
            Comment = "comment",
            Date = "2025-01-01",
            Version = "Version",
            Email = "me@email.com",
            Homepage = "https://example.com",
            Url = "https://example.com/database",
            ForceCalculateChecksums = true,
        };

        // Act
        await sut.CreateDatabaseAsync(sourceFolder.Path, databaseFilePath, databaseCreateSettings, CancellationToken.None);

        // Assert
        File.Exists(databaseFilePath).Should().BeTrue();
        var database = new ClrMameXmlImporter().Import(File.ReadAllText(databaseFilePath));
        database.Name.Should().Be("testdb");
        database.Author.Should().Be("author");
        database.Description.Should().Be("description");
        database.Category.Should().Be("category");
        database.Comment.Should().Be("comment");
        database.Date.Should().Be("2025-01-01");
        database.Version.Should().Be("Version");
        database.Email.Should().Be("me@email.com");
        database.Homepage.Should().Be("https://example.com");
        database.Url.Should().Be("https://example.com/database");
        database.Games.Should().ContainSingle();
        database.Games[0].Name.Should().Be("game2");
        database.Games[0].Roms.Should().HaveCount(2);
        database.Games[0].Roms[0].Name.Should().Be(sourceFile2a.FileName);
        database.Games[0].Roms[0].Size.Should().Be(sourceFile2a.Size);
        database.Games[0].Roms[1].Name.Should().Be(sourceFile2b.FileName);
        database.Games[0].Roms[1].Size.Should().Be(sourceFile2b.Size);

        added.Should().BeEquivalentTo([
            new RomAndFileMoniker(
                new RomMoniker("game2", "game2-a.bin"),
                new FileMoniker(sourceFile2a.FolderPath, sourceFile2a.FolderName, sourceFile2a.FileName, false)),
            new RomAndFileMoniker(
                new RomMoniker("game2", "game2-b.bin"),
                new FileMoniker(sourceFile2b.FolderPath, sourceFile2b.FolderName, sourceFile2b.FileName, false)),
        ]);

        progressPercentage.Should().Be(100);
        statuses.Take(2).Should().BeEquivalentTo([
            DatabaseCreateStatus.Pending,
            DatabaseCreateStatus.Started,
        ]);
        statuses.Last().Should().Be(DatabaseCreateStatus.Completed);
    }

    [Fact]
    public async Task GetDatabase()
    {
        // Arrange
        using var databasesFolder = new DisposableTempFolder();
        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = new Dictionary<string, object?>
            {
                { KnownSettingKeys.DatabaseFolders, new string[] { databasesFolder.Path } },
            },
        };
        new DatabaseBuilder()
            .WithName("test")
            .WithGame(g => g
                .WithName("game3")
                .WithRomFile("file3.bin", 500, "bbff86e6", "43f8df1895dc415f1b69b52a3804a500", "738ed16508d3faec2885e65d58135a1d4b3c7500", "b2ca48cd022b7b2c9fe3290eb9542928c46322319725b9fb7877cc27996d3a24"))
            .Build(Path.Combine(databasesFolder.Path, "db.dat"));

        var offlineExplorerService = new TestOfflineExplorerService();

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, offlineExplorerService);
        var databaseFile = new DatabaseFile { RootAbsoluteFolderPath = databasesFolder.Path, RelativePath = "db.dat" };

        // Act
        var database = await sut.GetDatabaseAsync(databaseFile, CancellationToken.None);

        // Assert
        database.Should().NotBeNull();
        database.Name.Should().Be("test");
        database.Games.Should().ContainSingle();
        database.Games[0].Name.Should().Be("game3");
        database.Games[0].Roms.Should().ContainSingle();
        database.Games[0].Roms[0].Name.Should().Be("file3.bin");
        database.Games[0].Roms[0].Size.Should().Be(500);
    }

    [Fact]
    public void GetRebuildTargetContainerTypes()
    {
        // Arrange
        var sut = new DatabaseService(new TestLogger<DatabaseService>(), new TestLocalSettingsService(), new TestOfflineExplorerService());

        // Act
        var actual = sut.GetRebuildTargetContainerTypes();

        // Assert
        actual.Should().Contain(KnownContainerTypes.Folder, KnownContainerTypes.Zip);
    }

    [Fact]
    public async Task GetFileScanSettings()
    {
        // Arrange
        using var scanFolder = new DisposableTempFolder();

        var dbFileScanSettings = new DatabaseFileScanSettings
        {
            UseOnlineFolders = true,
            ScanOnlineFolders =
            [
                new()
                {
                    FolderPath = scanFolder.Path,
                    IsIncluded = true,
                },
            ],
        };

        using var databasesFolder = new DisposableTempFolder();
        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = new Dictionary<string, object?>
            {
                { KnownSettingKeys.DatabaseFolders, new string[] { databasesFolder.Path } },
                { KnownSettingKeys.DatabaseFileScanSettings, new DatabaseScopeSetting<DatabaseFileScanSettings> { Entries = { ["test"] = dbFileScanSettings } } },
            },
        };

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, new TestOfflineExplorerService());

        // Act
        var actual = await sut.GetFileScanSettingsAsync("test", CancellationToken.None);

        // Assert
        actual.Should().BeEquivalentTo(dbFileScanSettings);
    }

    [Fact]
    public async Task SetFileScanSettings()
    {
        // Arrange
        using var scanFolder = new DisposableTempFolder();

        var dbFileScanSettings = new DatabaseFileScanSettings
        {
            UseOnlineFolders = true,
            ScanOnlineFolders =
            [
                new()
                {
                    FolderPath = scanFolder.Path,
                    IsIncluded = true,
                },
            ],
        };

        var localSettingsService = new TestLocalSettingsService();

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, new TestOfflineExplorerService());

        // Act
        await sut.SetFileScanSettingsAsync("test", dbFileScanSettings, CancellationToken.None);

        // Assert
        var actual = await sut.GetFileScanSettingsAsync("test", CancellationToken.None);
        actual.Should().BeEquivalentTo(dbFileScanSettings);

        var scopedSetting = (DatabaseScopeSetting<DatabaseFileScanSettings>?)localSettingsService.Settings[KnownSettingKeys.DatabaseFileScanSettings];
        scopedSetting.Should().NotBeNull();
        scopedSetting.Entries.Should().ContainKey("test");
        scopedSetting.Entries["test"].Should().BeEquivalentTo(dbFileScanSettings);
    }

    [Fact]
    public async Task GetFolderScanSettings()
    {
        // Arrange
        using var scanFolder = new DisposableTempFolder();

        var dbFolderScanSettings = new DatabaseFolderScanSettings
        {
            UseOnlineFolders = true,
            ScanOnlineFolders =
            [
                new()
                {
                    FolderPath = scanFolder.Path,
                    IsIncluded = true,
                },
            ],
        };

        using var databasesFolder = new DisposableTempFolder();
        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = new Dictionary<string, object?>
            {
                { KnownSettingKeys.DatabaseFolders, new string[] { databasesFolder.Path } },
                { KnownSettingKeys.DatabaseFolderScanSettings, new DatabaseScopeSetting<DatabaseFolderScanSettings> { Entries = { ["test"] = dbFolderScanSettings } } },
            },
        };

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, new TestOfflineExplorerService());

        // Act
        var actual = await sut.GetFolderScanSettingsAsync("test", CancellationToken.None);

        // Assert
        actual.Should().BeEquivalentTo(dbFolderScanSettings);
    }

    [Fact]
    public async Task SetFolderScanSettings()
    {
        // Arrange
        using var scanFolder = new DisposableTempFolder();

        var dbFolderScanSettings = new DatabaseFolderScanSettings
        {
            UseOnlineFolders = true,
            ScanOnlineFolders =
            [
                new()
                {
                    FolderPath = scanFolder.Path,
                    IsIncluded = true,
                },
            ],
        };

        var localSettingsService = new TestLocalSettingsService();
        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, new TestOfflineExplorerService());

        // Act
        await sut.SetFolderScanSettingsAsync("test", dbFolderScanSettings, CancellationToken.None);

        // Assert
        var actual = await sut.GetFolderScanSettingsAsync("test", CancellationToken.None);
        actual.Should().BeEquivalentTo(dbFolderScanSettings);

        var scopedSetting = (DatabaseScopeSetting<DatabaseFolderScanSettings>?)localSettingsService.Settings[KnownSettingKeys.DatabaseFolderScanSettings];
        scopedSetting.Should().NotBeNull();
        scopedSetting.Entries.Should().ContainKey("test");
        scopedSetting.Entries["test"].Should().BeEquivalentTo(dbFolderScanSettings);
    }

    [Fact]
    public async Task GetCueFolders()
    {
        // Arrange
        using var cueFolder1 = new DisposableTempFolder();
        using var cueFolder2 = new DisposableTempFolder();

        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = new Dictionary<string, object?>
            {
                { KnownSettingKeys.CueFolders, new string[] { cueFolder1.Path, cueFolder2.Path } },
            },
        };

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, new TestOfflineExplorerService());

        // Act
        var actual = await sut.GetCueFoldersAsync(CancellationToken.None);

        // Assert
        actual.Should().BeEquivalentTo([cueFolder1.Path, cueFolder2.Path]);
    }

    [Fact]
    public async Task AddCueFolder()
    {
        // Arrange
        using var cueFolder1 = new DisposableTempFolder();
        using var cueFolder2 = new DisposableTempFolder();

        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = new Dictionary<string, object?>
            {
                { KnownSettingKeys.CueFolders, new string[] { cueFolder1.Path } },
            },
        };

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, new TestOfflineExplorerService());

        // Act
        await sut.AddCueFolderAsync(cueFolder2.Path, CancellationToken.None);

        // Assert
        var actual = await sut.GetCueFoldersAsync(CancellationToken.None);
        actual.Should().BeEquivalentTo([cueFolder1.Path, cueFolder2.Path]);
    }

    [Fact]
    public async Task RemoveCueFolder()
    {
        // Arrange
        using var cueFolder1 = new DisposableTempFolder();
        using var cueFolder2 = new DisposableTempFolder();
        using var cueFolder3 = new DisposableTempFolder();

        var localSettingsService = new TestLocalSettingsService()
        {
            Settings = new Dictionary<string, object?>
            {
                { KnownSettingKeys.CueFolders, new string[] { cueFolder1.Path, cueFolder2.Path, cueFolder3.Path } },
            },
        };

        var sut = new DatabaseService(new TestLogger<DatabaseService>(), localSettingsService, new TestOfflineExplorerService());

        // Act
        await sut.RemoveCueFolderAsync(cueFolder1.Path, CancellationToken.None);

        // Assert
        var actual = await sut.GetCueFoldersAsync(CancellationToken.None);
        actual.Should().BeEquivalentTo([cueFolder2.Path, cueFolder3.Path]);
    }

    internal class TestOfflineExplorerService : IOfflineExplorerService
    {
#pragma warning disable CS0067
        public event EventHandler? RepositoryChanged;

        public event EventHandler<OfflineDiskCreateProgressEventArgs>? DiskCreateProgress;
#pragma warning restore CS0067

        public Task AddFolderAsync(string folderPath, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task CreateDiskAsync(string sourceFolderPath, string targetDiskFilePath, string diskName, OfflineDiskCreateSettings settings, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<OfflineDisk?> FindDiskByNameAsync(string name, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<OfflineDisk?> GetDiskAsync(OfflineDiskFile file, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<string[]> GetFoldersAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<OfflineRepository> GetOfflineRepositoryAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFolderAsync(string folderPath, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }

    private class RebuiltFileResult
    {
        public RebuiltFileResult(string name, RandomFile source)
        {
            this.Name = name;
            this.Source = source;
        }

        public string Name { get; }

        public RandomFile Source { get; }
    }

    private class RebuiltResult
    {
        public RebuiltResult(string name, string parentFolder, string containerType, RebuiltFileResult[] entries)
        {
            this.Name = name;
            this.ParentFolder = parentFolder;
            this.ContainerType = containerType;
            this.Entries = [.. entries];
        }

        public string Name { get; }

        public string ParentFolder { get; set; }

        public string ContainerType { get; set; }

        public List<RebuiltFileResult> Entries { get; }

        public string FullPath => Path.Combine(this.ParentFolder, this.NameWithExtension);

        public string NameWithExtension
        {
            get
            {
                return this.ContainerType switch
                {
                    KnownContainerTypes.SevenZip => $"{this.Name}.7z",
                    KnownContainerTypes.TorrentSevenZip => $"{this.Name}.7z",
                    KnownContainerTypes.Zip => $"{this.Name}.zip",
                    KnownContainerTypes.TorrentZip => $"{this.Name}.zip",
                    KnownContainerTypes.Folder => this.Name,
                    _ => throw new NotSupportedException($"Unsupported container type: {this.ContainerType}"),
                };
            }
        }
    }
}
