// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

using System.Xml.Serialization;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(IsNullable = false)]
public class DiskArea
{
    [XmlElement("disk")]
    public Disk[] Disk { get; set; } = [];

    [XmlAttribute]
    public string Name { get; set; } = string.Empty;
}
