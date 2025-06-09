// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

using System.Xml.Serialization;

[Serializable]
[XmlType(AnonymousType = true, Namespace = "")]
[XmlRoot(Namespace = "", IsNullable = false, ElementName = "softwarelist")]
public class SoftwareList
{
    [XmlElement("notes")]
    public string? Notes { get; set; }

    [XmlElement("software")]
    public List<Software> Software { get; set; } = [];

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("description")]
    public string Description { get; set; } = string.Empty;
}
