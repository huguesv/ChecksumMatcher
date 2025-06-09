// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

using System.ComponentModel;
using System.Xml.Serialization;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(IsNullable = false)]
public class DataArea
{
    public DataArea()
    {
        this.Width = DataAreaWidth.Item8;
        this.Endianness = DataAreaEndianness.Little;
    }

    [XmlElement("rom")]
    public List<Rom> Rom { get; set; } = [];

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("size")]
    public string? Size { get; set; }

    [XmlAttribute("width")]
    [DefaultValue(DataAreaWidth.Item8)]
    public DataAreaWidth Width { get; set; }

    [XmlAttribute("endianness")]
    [DefaultValue(DataAreaEndianness.Little)]
    public DataAreaEndianness Endianness { get; set; }
}
