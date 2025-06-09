// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

internal sealed class FileExplorerService : IFileExplorerService
{
    public void OpenInExplorer(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (Directory.Exists(path))
            {
                Process.Start("explorer", $"\"{path}\"");
            }
            else if (File.Exists(path))
            {
                Process.Start("explorer", $"/select,\"{path}\"");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", $"-R \"{path}\"");
        }
        else
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Path.GetDirectoryName(path),
                UseShellExecute = true,
            });
        }
    }
}
