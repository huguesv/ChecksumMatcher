// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.UnitTest.Infrastructure;

using System;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

internal class OfflineDiskBuilder
{
    public OfflineDisk Build()
    {
        var result = new OfflineDisk
        {
            Name = "Disk1",
            Label = "Label1",
            RootFolders = [
                new OfflineItem
                    {
                        Name = @"C:\",
                        Path = @"C:\",
                        Kind = OfflineItemKind.Folder,
                        Items = [
                        new OfflineItem
                            {
                                Name = "Data",
                                Path = @"C:\Data",
                                Kind = OfflineItemKind.Folder,
                                Items = [
                                    new OfflineItem
                                    {
                                        Name = "SubFolder",
                                        Path = @"C:\Data\SubFolder",
                                        Created = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                                        Kind = OfflineItemKind.Folder,
                                        Items = [
                                            new OfflineItem
                                            {
                                                Name = "SubFile1.txt",
                                                Path = @"C:\Data\SubFolder\SubFile1.txt",
                                                Created = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                                                Kind = OfflineItemKind.File,
                                                Size = 500,
                                            },
                                            new OfflineItem
                                            {
                                                Name = "SubFile2.txt",
                                                Path = @"C:\Data\SubFolder\SubFile2.txt",
                                                Created = new DateTime(2023, 2, 1, 12, 0, 0, DateTimeKind.Utc),
                                                Kind = OfflineItemKind.File,
                                                Size = 2048,
                                            }
                                        ],
                                    },
                                    new OfflineItem
                                    {
                                        Name = "File1.txt",
                                        Path = @"C:\Data\File1.txt",
                                        Created = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                                        Kind = OfflineItemKind.File,
                                        Size = 500,
                                    },
                                    new OfflineItem
                                    {
                                        Name = "File2.bin",
                                        Path = @"C:\Data\File2.bin",
                                        Created = new DateTime(2023, 2, 1, 12, 0, 0, DateTimeKind.Utc),
                                        Kind = OfflineItemKind.File,
                                        Size = 2048,
                                    }
                                ],
                            }
                        ],
                    }
            ],
        };

        return result;
    }
}
