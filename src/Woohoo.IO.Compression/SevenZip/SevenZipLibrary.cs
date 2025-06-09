// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.SevenZip;

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using global::SevenZip;

public static class SevenZipLibrary
{
    private static readonly object IsInitializedLock = new();

    private static bool isInitialized;

    public static void Initialize()
    {
        lock (IsInitializedLock)
        {
            if (isInitialized)
            {
                return;
            }

            string folderName;
            var processArchitecture = RuntimeInformation.ProcessArchitecture;
            switch (processArchitecture)
            {
                case Architecture.X86:
                    folderName = "x86";
                    break;
                case Architecture.X64:
                    folderName = "x64";
                    break;
                case Architecture.Arm64:
                    folderName = "arm64";
                    break;
                default:
                    throw new FileNotFoundException($"The platform is not supported for 7z: {processArchitecture}");
            }

            var currentFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NotSupportedException();
            var binaryFilePath = Path.Combine(currentFolderPath, folderName, "7z.dll");
            if (!File.Exists(binaryFilePath))
            {
                throw new FileNotFoundException($"The 7z.dll file was not found at the expected path: {binaryFilePath}");
            }

            SevenZipBase.SetLibraryPath(binaryFilePath);

            isInitialized = true;
        }
    }
}
