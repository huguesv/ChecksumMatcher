// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using Woohoo.ChecksumDatabase.Model;

public sealed record class MergedDisk
{
    public MergedDisk()
    {
        this.OriginalParentGameName = string.Empty;
        this.Name = string.Empty;
        this.MD5 = [];
        this.SHA1 = [];
        this.Merge = string.Empty;
        this.Status = RomStatus.Good;
    }

    public string OriginalParentGameName { get; set; }

    public string Name { get; set; }

    public byte[] MD5 { get; set; }

    public byte[] SHA1 { get; set; }

    public string Merge { get; set; }

    public RomStatus Status { get; set; }
}
