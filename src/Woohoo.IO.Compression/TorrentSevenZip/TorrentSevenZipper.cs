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
        Debug.Assert(!string.IsNullOrEmpty(targetArchiveFilePath), "Null or empty targetArchiveFilePath.");
        Debug.Assert(!string.IsNullOrEmpty(sourceFilePath), "Null or empty sourceFilePath.");

        var torrentzipFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NotSupportedException(), "t7z.exe");

        var startInfo = new ProcessStartInfo
        {
            FileName = torrentzipFilePath,
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
        Debug.Assert(!string.IsNullOrEmpty(targetArchiveFilePath), "Null or empty targetArchiveFilePath.");
        Debug.Assert(expectedTargetFiles != null, "Null expectedTargetFiles.");

        if (isCompleteDelegate(targetArchiveFilePath, expectedTargetFiles))
        {
            var torrentzipFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NotSupportedException(), "t7z.exe");
            return Torrentzip(targetArchiveFilePath, expectedTargetFiles, torrentzipFilePath);
        }

        return false;
    }

    private static bool Torrentzip(string targetArchiveFilePath, string[] expectedTargetFiles, string torrentzipFilePath)
    {
        Debug.Assert(!string.IsNullOrEmpty(targetArchiveFilePath), "Null or empty targetArchiveFilePath.");
        Debug.Assert(expectedTargetFiles != null, "Null expectedTargetFiles.");
        Debug.Assert(!string.IsNullOrEmpty(torrentzipFilePath), "Null or empty torrentzipFilePath.");

        var startInfo = new ProcessStartInfo
        {
            FileName = torrentzipFilePath,
            Arguments = "\"" + targetArchiveFilePath + "\"",
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        var process = Process.Start(startInfo);
        process?.WaitForExit();

        return process?.ExitCode == 0;
    }
}
