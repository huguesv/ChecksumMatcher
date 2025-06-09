// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.TorrentSevenZip;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]
public static class TorrentSevenZipper
{
    private static string ExecutableFilePath
    {
        get
        {
            var torrentzipFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NotSupportedException(), "t7z.exe");
            if (!File.Exists(torrentzipFilePath))
            {
                throw new FileNotFoundException("t7z.exe was not found.", torrentzipFilePath);
            }

            return torrentzipFilePath;
        }
    }

    public static async Task Create7zAsync(string targetArchiveFilePath, string sourceFilePath, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);
        ArgumentException.ThrowIfNullOrEmpty(sourceFilePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = ExecutableFilePath,
            WorkingDirectory = Path.GetDirectoryName(targetArchiveFilePath) ?? throw new NotSupportedException(),
            Arguments = "a \"" + targetArchiveFilePath + "\" \"" + sourceFilePath + "\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        await StartAndWaitAsync(startInfo, targetArchiveFilePath, ct);
    }

    public static async Task TorrentZipAsync(string targetArchiveFilePath, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = ExecutableFilePath,
            WorkingDirectory = Path.GetDirectoryName(targetArchiveFilePath) ?? throw new NotSupportedException(),
            Arguments = "\"" + targetArchiveFilePath + "\"",
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

        try
        {
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

            if (!string.IsNullOrEmpty(stderr) || (process.ExitCode != ExitCodes.Success && process.ExitCode != ExitCodes.UserError))
            {
                string exceptionDetails = $"\nOutput:\n{stdout}\n\nError:\n{stderr}";
                throw new CompressionException($"t7z process failed with exit code {process.ExitCode} for file {targetArchiveFilePath}. {exceptionDetails}");
            }
        }
        catch (OperationCanceledException)
        {
            var tempFilePath = Path.ChangeExtension(targetArchiveFilePath, ".7z.tmp");
            if (File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch
                {
                }
            }

            var targetFolderPath = Path.GetDirectoryName(targetArchiveFilePath);
            if (Directory.Exists(targetFolderPath))
            {
                foreach (var folderPath in Directory.GetDirectories(targetFolderPath, "t7z*.tmp", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        Directory.Delete(folderPath, true);
                    }
                    catch
                    {
                    }
                }
            }

            throw;
        }
    }

    // From https://github.com/tikki/t7z/blob/master/src/cpp/7zip/UI/Common/ExitCode.h
    private static class ExitCodes
    {
        public const int Success = 0; // Successful operation
        public const int Warning = 1; // Non fatal error(s) occurred
        public const int FatalError = 2; // A fatal error occurred
        public const int UserError = 7; // Command line option error
        public const int MemoryError = 8; // Not enough memory for operation
        public const int UserBreak = 255; // User stopped the process
    }
}
