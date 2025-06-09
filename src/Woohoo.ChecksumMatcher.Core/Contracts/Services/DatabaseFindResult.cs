// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using Woohoo.ChecksumDatabase.Model;

public class DatabaseFindResult
{
    public DatabaseFindResult(RomDatabase? db, string rootFolder, string absoluteFilePath, string relativeFilePath)
    {
        this.Database = db;
        this.RootFolder = rootFolder;
        this.AbsoluteFilePath = absoluteFilePath;
        this.RelativeFilePath = relativeFilePath;
    }

    public RomDatabase? Database { get; }

    public string RootFolder { get; }

    public string AbsoluteFilePath { get; }

    public string RelativeFilePath { get; }
}
