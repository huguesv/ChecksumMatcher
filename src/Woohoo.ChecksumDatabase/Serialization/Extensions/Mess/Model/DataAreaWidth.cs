// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

using System.Xml.Serialization;

[Serializable]
[XmlType(AnonymousType = true)]
public enum DataAreaWidth
{
    [XmlEnum("8")]
    Item8,

    [XmlEnum("16")]
    Item16,

    [XmlEnum("32")]
    Item32,

    [XmlEnum("64")]
    Item64,
}
