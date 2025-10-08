// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

using System.ComponentModel;
using System.Xml.Serialization;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(IsNullable = false)]
public class Software
{
    public Software()
    {
        this.Supported = SoftwareSupported.Yes;
    }

    [XmlElement("description")]
    public string? Description { get; set; }

    [XmlElement("year")]
    public string? Year { get; set; }

    [XmlElement("publisher")]
    public string? Publisher { get; set; }

    [XmlElement("notes")]
    public string? Notes { get; set; }

    [XmlElement("info")]
    public Info[] Info { get; set; } = [];

    [XmlElement("sharedfeat")]
    public SharedFeat[] SharedFeat { get; set; } = [];

    [XmlElement("part")]
    public Part[] Part { get; set; } = [];

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("cloneof")]
    public string? Cloneof { get; set; }

    [XmlAttribute("supported")]
    [DefaultValue(SoftwareSupported.Yes)]
    public SoftwareSupported Supported { get; set; }
}
