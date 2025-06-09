// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

using System.ComponentModel;
using System.Xml.Serialization;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(IsNullable = false)]
public class Rom
{
    public Rom()
    {
        this.Status = RomStatus.Good;
    }

    [XmlAttribute("name")]
    public string? Name { get; set; }

    [XmlAttribute("size")]
    public string? Size { get; set; }

    [XmlAttribute("crc")]
    public string? Crc { get; set; }

    [XmlAttribute("sha1")]
    public string? Sha1 { get; set; }

    [XmlAttribute("offset")]
    public string? Offset { get; set; }

    [XmlAttribute("value")]
    public string? Value { get; set; }

    [XmlAttribute("status")]
    [DefaultValue(RomStatus.Good)]
    public RomStatus Status { get; set; }

    [XmlAttribute("loadflag")]
    public RomLoadFlag LoadFlag { get; set; }

    [XmlAttribute("loadflag")]
    [XmlIgnore]
    public bool LoadFlagSpecified { get; set; }
}
