// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.TorrentZip;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]
public static class TorrentZipper
{
    private static string ExecutableFilePath
    {
        get
        {
            var torrentzipFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NotSupportedException(), "trrntzip.exe");
            if (!File.Exists(torrentzipFilePath))
            {
                throw new FileNotFoundException("trrntzip.exe was not found.", torrentzipFilePath);
            }

            return torrentzipFilePath;
        }
    }

    public static async Task TorrentZipAsync(string targetArchiveFilePath, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);

        if (!string.Equals(Path.GetExtension(targetArchiveFilePath), ".zip", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"The target archive file path '{targetArchiveFilePath}' must have a .zip extension.", nameof(targetArchiveFilePath));
        }

        var executableFilePath = ExecutableFilePath;
        var logFilePath = Path.Combine(Path.GetDirectoryName(executableFilePath) ?? throw new NotSupportedException(), "error.log");
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = executableFilePath,
            WorkingDirectory = Path.GetDirectoryName(targetArchiveFilePath) ?? throw new NotSupportedException(),
            Arguments = "-g \"" + targetArchiveFilePath + "\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        await StartAndWaitAsync(startInfo, targetArchiveFilePath, ct);
    }

    private static async Task StartAndWaitAsync(ProcessStartInfo startInfo, string targetArchiveFilePath, CancellationToken ct)
    {
        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start {startInfo.FileName}");

        using var cancellationTokenRegistration = ct.Register(() =>
        {
            if (process.HasExited == false)
            {
                process.Kill(entireProcessTree: true);
            }
        });

        var outputTask = process.StandardOutput.ReadToEndAsync(ct);
        var errorTask = process.StandardError.ReadToEndAsync(ct);

        await process.WaitForExitAsync(ct);

        var stdout = await outputTask;
        var stderr = await errorTask;

        if (!string.IsNullOrEmpty(stderr) || process.ExitCode != 0)
        {
            string exceptionDetails = $"\nOutput:\n{stdout}\n\nError:\n{stderr}";
            throw new CompressionException($"trrntzip process failed with exit code {process.ExitCode} for file {targetArchiveFilePath}. {exceptionDetails}");
        }
    }
}
