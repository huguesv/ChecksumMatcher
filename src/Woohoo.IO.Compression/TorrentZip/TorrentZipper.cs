// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.TorrentZip;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

public static class TorrentZipper
{
    public static bool Torrentzip(string targetArchiveFilePath, string[] expectedTargetFiles, Func<string, string[], bool> isCompleteDelegate)
    {
        Debug.Assert(!string.IsNullOrEmpty(targetArchiveFilePath), "Null or empty targetArchiveFilePath.");
        Debug.Assert(expectedTargetFiles != null, "Null expectedTargetFiles.");

        if (isCompleteDelegate(targetArchiveFilePath, expectedTargetFiles))
        {
            var torrentzipFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NotSupportedException(), "trrntzip.exe");
            Torrentzip(targetArchiveFilePath, expectedTargetFiles, torrentzipFilePath);

            return true;
        }

        return false;
    }

    private static void Torrentzip(string targetArchiveFilePath, string[] expectedTargetFiles, string torrentzipFilePath)
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
    }
}
