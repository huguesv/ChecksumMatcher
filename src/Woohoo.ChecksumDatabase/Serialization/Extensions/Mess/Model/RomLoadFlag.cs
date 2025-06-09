// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

using System.Xml.Serialization;

[Serializable]
[XmlType(AnonymousType = true)]
public enum RomLoadFlag
{
    [XmlEnum("load16_byte")]
    Load16Byte,

    [XmlEnum("load16_word")]
    Load16Word,

    [XmlEnum("load16_word_swap")]
    Load16WordSwap,

    [XmlEnum("load32_byte")]
    Load32Byte,

    [XmlEnum("load32_word")]
    Load32Word,

    [XmlEnum("load32_word_swap")]
    Load32WordSwap,

    [XmlEnum("load32_dword")]
    Load32DoubleWord,

    [XmlEnum("load64_word")]
    Load64Word,

    [XmlEnum("load64_word_swap")]
    Load64WordSwap,

    [XmlEnum("reload")]
    Reload,

    [XmlEnum("fill")]
    Fill,

    [XmlEnum("continue")]
    Continue,

    [XmlEnum("reload_plain")]
    ReloadPlain,

    [XmlEnum("ignore")]
    Ignore,
}
