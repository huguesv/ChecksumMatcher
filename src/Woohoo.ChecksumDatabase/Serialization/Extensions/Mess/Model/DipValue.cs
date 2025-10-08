// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

using System.ComponentModel;
using System.Xml.Serialization;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(IsNullable = false)]
public class DipValue
{
    public DipValue()
    {
        this.Default = DipValueDefault.No;
    }

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("value")]
    public string Value { get; set; } = string.Empty;

    [XmlAttribute("default")]
    [DefaultValue(DipValueDefault.No)]
    public DipValueDefault Default { get; set; }
}
