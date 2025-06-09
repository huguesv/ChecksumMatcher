// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Helpers;

using System.IO;

public static class FileUtility
{
    public static void SafeDelete(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                var attrs = File.GetAttributes(filePath);
                if ((attrs & FileAttributes.ReadOnly) != 0)
                {
                    File.SetAttributes(filePath, attrs & ~FileAttributes.ReadOnly);
                }

                File.Delete(filePath);
            }
        }
        catch (IOException)
        {
        }
    }

    public static void SafeDeleteFolder(string folderPath)
    {
        try
        {
            Directory.Delete(folderPath);
        }
        catch (IOException)
        {
        }
    }

    public static void SafeDeleteFolder(string folderPath, bool recursive)
    {
        try
        {
            Directory.Delete(folderPath, recursive);
        }
        catch (IOException)
        {
        }
    }
}
