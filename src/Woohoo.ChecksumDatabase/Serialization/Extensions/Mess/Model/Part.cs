// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

using System.Xml.Serialization;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(IsNullable = false)]
public class Part
{
    [XmlElement("feature")]
    public List<Feature> Feature { get; set; } = [];

    [XmlElement("dataarea")]
    public List<DataArea> DataArea { get; set; } = [];

    [XmlElement("diskarea")]
    public List<DiskArea> DiskArea { get; set; } = [];

    [XmlElement("dipswitch")]
    public List<DipSwitch> DipSwitch { get; set; } = [];

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("interface")]
    public string? Interface { get; set; }
}
