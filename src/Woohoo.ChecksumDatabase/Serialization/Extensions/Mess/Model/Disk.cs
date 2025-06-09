// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

using System.ComponentModel;
using System.Xml.Serialization;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(IsNullable = false)]
public class Disk
{
    public Disk()
    {
        this.Status = DiskStatus.Good;
        this.Writeable = DiskWriteable.No;
    }

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("sha1")]
    public string? Sha1 { get; set; }

    [XmlAttribute("status")]
    [DefaultValue(DiskStatus.Good)]
    public DiskStatus Status { get; set; }

    [XmlAttribute("writeable")]
    [DefaultValue(DiskWriteable.No)]
    public DiskWriteable Writeable { get; set; }
}
