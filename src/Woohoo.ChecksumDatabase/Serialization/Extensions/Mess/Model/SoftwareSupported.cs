// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

using System.Xml.Serialization;

[Serializable]
[XmlType(AnonymousType = true)]
public enum SoftwareSupported
{
    [XmlEnum("yes")]
    Yes,

    [XmlEnum("partial")]
    Partial,

    [XmlEnum("no")]
    No,
}
