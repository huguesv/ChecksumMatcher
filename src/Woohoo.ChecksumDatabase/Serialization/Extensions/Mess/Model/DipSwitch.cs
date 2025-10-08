// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

using System.Xml.Serialization;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(IsNullable = false)]
public class DipSwitch
{
    [XmlElement("dipvalue")]
    public DipValue[] DipValue { get; set; } = [];

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("tag")]
    public string Tag { get; set; } = string.Empty;

    [XmlAttribute("mask")]
    public string Mask { get; set; } = string.Empty;
}
