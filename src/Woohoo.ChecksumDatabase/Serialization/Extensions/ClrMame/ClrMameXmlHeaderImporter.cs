// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Woohoo.ChecksumDatabase.Model;

internal static class ClrMameXmlHeaderImporter
{
    public static void Import(string text, RomHeader header)
    {
        Requires.NotNull(text);
        Requires.NotNull(header);

        var settings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true,
            DtdProcessing = DtdProcessing.Ignore,
        };

        using (var input = new StringReader(text))
        {
            var reader = XmlReader.Create(input, settings);
            _ = reader.MoveToContent();
            if (reader.Name == "detector")
            {
                ReadDetector(reader, header);
            }
        }
    }

    private static void ReadDetector(XmlReader reader, RomHeader header)
    {
        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();

            while (reader.Name != "detector")
            {
                switch (reader.Name)
                {
                    case "name":
                        header.Name = reader.ReadElementContentAsString();
                        break;
                    case "author":
                        header.Author = reader.ReadElementContentAsString();
                        break;
                    case "version":
                        header.Version = reader.ReadElementContentAsString();
                        break;
                    case "rule":
                        ReadRule(reader, header);
                        break;
                }
            }

            reader.ReadEndElement();
        }
    }

    private static void ReadRule(XmlReader reader, RomHeader header)
    {
        var rule = new RomHeaderRule();

        var startOffset = reader.GetAttribute("start_offset");
        if (!string.IsNullOrEmpty(startOffset))
        {
            rule.StartOffset = long.Parse(startOffset, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        }

        var endOffset = reader.GetAttribute("end_offset");
        if (!string.IsNullOrEmpty(endOffset))
        {
            rule.EndOffset = string.Compare(endOffset, "eof", StringComparison.OrdinalIgnoreCase) == 0
                ? long.MaxValue
                : long.Parse(endOffset, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        }

        var operation = reader.GetAttribute("operation");
        if (!string.IsNullOrEmpty(operation))
        {
            switch (operation.ToUpperInvariant())
            {
                case "NONE":
                    rule.Operation = RomHeaderRuleOperation.None;
                    break;

                case "BITSWAP":
                    rule.Operation = RomHeaderRuleOperation.BitSwap;
                    break;

                case "BYTESWAP":
                    rule.Operation = RomHeaderRuleOperation.ByteSwap;
                    break;

                case "WORDSWAP":
                    rule.Operation = RomHeaderRuleOperation.WordSwap;
                    break;

                case "WORDBYTESWAP":
                    rule.Operation = RomHeaderRuleOperation.WordByteSwap;
                    break;

                default:
                    throw new DatabaseImportException(string.Format(CultureInfo.CurrentCulture, "Invalid header rule operation '{0}'.", operation));
            }
        }

        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();

            while (reader.Name != "rule")
            {
                switch (reader.Name)
                {
                    case "data":
                        ReadDataTest(reader, rule);
                        break;

                    case "or":
                        ReadBooleanTest(reader, rule, RomHeaderBooleanTestOperation.Or);
                        break;

                    case "xor":
                        ReadBooleanTest(reader, rule, RomHeaderBooleanTestOperation.Xor);
                        break;

                    case "and":
                        ReadBooleanTest(reader, rule, RomHeaderBooleanTestOperation.And);
                        break;

                    case "file":
                        ReadFileTest(reader, rule);
                        break;
                }
            }

            reader.ReadEndElement();
        }

        header.Rules.Add(rule);
    }

    private static void ReadDataTest(XmlReader reader, RomHeaderRule rule)
    {
        var test = new RomHeaderDataTest();

        var offset = reader.GetAttribute("offset");
        if (!string.IsNullOrEmpty(offset))
        {
            test.Offset = long.Parse(offset, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        }

        var value = reader.GetAttribute("value");
        if (!string.IsNullOrEmpty(value))
        {
            test.SetValues(Hex.TextToByteArray(value));
        }

        var result = reader.GetAttribute("result");
        if (!string.IsNullOrEmpty(result))
        {
            test.Result = ParseResult(result);
        }

        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();
            reader.ReadEndElement();
        }

        rule.Tests.Add(test);
    }

    private static void ReadBooleanTest(XmlReader reader, RomHeaderRule rule, RomHeaderBooleanTestOperation operation)
    {
        var test = new RomHeaderBooleanTest
        {
            Operation = operation,
        };

        var offset = reader.GetAttribute("offset");
        if (!string.IsNullOrEmpty(offset))
        {
            test.Offset = long.Parse(offset, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        }

        var mask = reader.GetAttribute("mask");
        if (!string.IsNullOrEmpty(mask))
        {
            test.SetMasks(Hex.TextToByteArray(mask));
        }

        var value = reader.GetAttribute("value");
        if (!string.IsNullOrEmpty(value))
        {
            test.SetValues(Hex.TextToByteArray(value));
        }

        var result = reader.GetAttribute("result");
        if (!string.IsNullOrEmpty(result))
        {
            test.Result = ParseResult(result);
        }

        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();
            reader.ReadEndElement();
        }

        rule.Tests.Add(test);
    }

    private static void ReadFileTest(XmlReader reader, RomHeaderRule rule)
    {
        var test = new RomHeaderFileTest();

        var size = reader.GetAttribute("size");
        if (!string.IsNullOrEmpty(size))
        {
            if (string.Compare(size, "po2", StringComparison.OrdinalIgnoreCase) == 0)
            {
                test.SizeMode = RomHeaderFileSizeMode.PowerOf2;
                test.Size = 0;
            }
            else
            {
                test.SizeMode = RomHeaderFileSizeMode.Fixed;
                test.Size = long.Parse(size, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            }
        }

        var operation = reader.GetAttribute("operator");
        if (!string.IsNullOrEmpty(operation))
        {
            switch (operation.ToUpperInvariant())
            {
                case "EQUAL":
                    test.Operation = RomHeaderFileTestOperation.Equal;
                    break;

                case "LESS":
                    test.Operation = RomHeaderFileTestOperation.Less;
                    break;

                case "GREATER":
                    test.Operation = RomHeaderFileTestOperation.Greater;
                    break;

                default:
                    throw new DatabaseImportException(string.Format(CultureInfo.CurrentCulture, "Invalid header file test operator '{0}'.", operation));
            }
        }

        var result = reader.GetAttribute("result");
        if (!string.IsNullOrEmpty(result))
        {
            test.Result = ParseResult(result);
        }

        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();
            reader.ReadEndElement();
        }

        rule.Tests.Add(test);
    }

    private static bool ParseResult(string result)
    {
        switch (result.ToUpperInvariant())
        {
            case "TRUE":
                return true;

            case "FALSE":
                return false;

            default:
                throw new DatabaseImportException(string.Format(CultureInfo.CurrentCulture, "Invalid header test result '{0}'.", result));
        }
    }
}
