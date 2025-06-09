// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.TorrentSevenZip;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

public static class TorrentSevenZipper
{
    public static bool Create7z(string targetArchiveFilePath, string sourceFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);
        ArgumentException.ThrowIfNullOrEmpty(sourceFilePath);

        var torrentzipFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NotSupportedException(), "t7z.exe");
        if (!File.Exists(torrentzipFilePath))
        {
            throw new FileNotFoundException("t7z.exe was not found.", torrentzipFilePath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = torrentzipFilePath,
            WorkingDirectory = Path.GetDirectoryName(targetArchiveFilePath) ?? throw new NotSupportedException(),
            Arguments = "a \"" + targetArchiveFilePath + "\" \"" + sourceFilePath + "\"",
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        var process = Process.Start(startInfo);
        process?.WaitForExit();

        return process?.ExitCode == 0;
    }

    public static bool Torrentzip(string targetArchiveFilePath, string[] expectedTargetFiles, Func<string, string[], bool> isCompleteDelegate)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);
        ArgumentNullException.ThrowIfNull(isCompleteDelegate);

        if (isCompleteDelegate(targetArchiveFilePath, expectedTargetFiles))
        {
            var torrentzipFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NotSupportedException(), "t7z.exe");
            if (!File.Exists(torrentzipFilePath))
            {
                throw new FileNotFoundException("t7z.exe was not found.", torrentzipFilePath);
            }

            return Torrentzip(targetArchiveFilePath, expectedTargetFiles, torrentzipFilePath);
        }

        return false;
    }

    private static bool Torrentzip(string targetArchiveFilePath, string[] expectedTargetFiles, string torrentzipFilePath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = torrentzipFilePath,
            WorkingDirectory = Path.GetDirectoryName(targetArchiveFilePath) ?? throw new NotSupportedException(),
            Arguments = "\"" + targetArchiveFilePath + "\"",
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        var process = Process.Start(startInfo);
        process?.WaitForExit();

        return process?.ExitCode == 0;
    }
}
