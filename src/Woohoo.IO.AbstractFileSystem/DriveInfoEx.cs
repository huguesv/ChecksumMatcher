// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

using System;

public static class DriveInfoEx
{
    public static unsafe string GetVolumeSerial(string driveRoot)
    {
        ArgumentException.ThrowIfNullOrEmpty(driveRoot);

        Span<char> volumeName = stackalloc char[1024];
        uint serial = 0;
        uint maxComponentLength = 0;
        uint fileSystemFlags = 0;
        Span<char> systemName = stackalloc char[1024];

        var res = Windows.Win32.PInvoke.GetVolumeInformation(driveRoot, volumeName, &serial, &maxComponentLength, &fileSystemFlags, systemName);
        if (res != 1)
        {
            throw new IOException("Unable to retrieve drive volume information.");
        }

        var result = $"{serial >> 24 & 0xff:X2}{serial >> 16 & 0xff:X2}-{serial >> 8 & 0xff:X2}{serial & 0xff:X2}";

        return result;
    }
}
