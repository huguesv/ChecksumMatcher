// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Services;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;

public sealed partial class RedumpArtifactsService : IRedumpArtifactsService
{
    public ImmutableArray<string> CleanupContents(string folderPath)
    {
        // Get all the artifacts from the directory, and look at the
        // version (date+time) in the file name to only keep the latest ones.
        var nameToLatestMap = new Dictionary<string, (string FilePath, DateTime DateTime)>();
        var artifactFilePaths = new List<string>();
        foreach (var filePath in Directory.EnumerateFiles(folderPath, "*", SearchOption.TopDirectoryOnly))
        {
            var match = RedumpArtifactFileNameRegex().Match(Path.GetFileName(filePath));
            if (match.Success)
            {
                var name = match.Groups[1].Value.Trim();
                var version = match.Groups[2].Value.Trim();
                if (DateTime.TryParseExact(version, "yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    artifactFilePaths.Add(filePath);
                    if (nameToLatestMap.TryGetValue(name, out var existingFile))
                    {
                        if (dt > existingFile.DateTime)
                        {
                            nameToLatestMap[name] = (filePath, dt);
                        }
                    }
                    else
                    {
                        nameToLatestMap[name] = (filePath, dt);
                    }
                }
            }
        }

        var deleteFilePaths = artifactFilePaths.Except(nameToLatestMap.Values.Select(f => f.FilePath));
        foreach (var filePath in deleteFilePaths)
        {
            FileUtility.SafeDelete(filePath);
        }

        return [.. deleteFilePaths];
    }

    public void DeleteContents(string folderPath)
    {
        foreach (var file in Directory.EnumerateFiles(folderPath))
        {
            FileUtility.SafeDelete(file);
        }

        foreach (var folder in Directory.EnumerateDirectories(folderPath))
        {
            FileUtility.SafeDeleteFolder(folder, recursive: true);
        }
    }

    [GeneratedRegex("^(.*?) \\([^\\)]*\\) \\(([^\\)]*)\\)\\.(zip|dat)$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RedumpArtifactFileNameRegex();
}
