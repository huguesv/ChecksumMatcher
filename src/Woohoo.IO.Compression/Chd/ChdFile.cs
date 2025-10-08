// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Chd;

using System;
using System.IO;
using System.Linq;
using System.Text;

public sealed class ChdFile
{
    public ChdFile(string diskFilePath)
    {
        this.DiskFilePath = diskFilePath;

        this.ReadHeader();
    }

    public string DiskFilePath { get; }

    public int FormatVersion { get; private set; }

    public byte[] SHA1 { get; private set; } = [];

    private void ReadHeader()
    {
        using var stream = File.OpenRead(this.DiskFilePath);

        byte[] tagBytes = new byte[8];
        stream.ReadExactly(tagBytes);

        if (Encoding.ASCII.GetString(tagBytes) != "MComprHD")
        {
            throw new InvalidOperationException("Not a CHD file.");
        }

        byte[] lengthBytes = new byte[4];
        stream.ReadExactly(lengthBytes);

        if (BitConverter.IsLittleEndian)
        {
            lengthBytes.Reverse();
        }

        int headerLength = BitConverter.ToInt32(lengthBytes);

        byte[] versionBytes = new byte[4];
        stream.ReadExactly(versionBytes);

        if (BitConverter.IsLittleEndian)
        {
            versionBytes.Reverse();
        }

        this.FormatVersion = BitConverter.ToInt32(versionBytes);

        switch (this.FormatVersion)
        {
            case 3:
                this.ReadHeaderV3(stream);
                break;
            case 4:
                this.ReadHeaderV4(stream);
                break;
            case 5:
                this.ReadHeaderV5(stream);
                break;
            default:
                throw new InvalidOperationException($"Unsupported CHD version {this.FormatVersion}.");
        }
    }

    private void ReadHeaderV3(FileStream stream)
    {
        stream.Seek(80, SeekOrigin.Begin);
        this.SHA1 = new byte[20];
        stream.ReadExactly(this.SHA1);
    }

    private void ReadHeaderV4(FileStream stream)
    {
        stream.Seek(48, SeekOrigin.Begin);
        this.SHA1 = new byte[20];
        stream.ReadExactly(this.SHA1);
    }

    private void ReadHeaderV5(FileStream stream)
    {
        stream.Seek(84, SeekOrigin.Begin);
        this.SHA1 = new byte[20];
        stream.ReadExactly(this.SHA1);
    }
}
