// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Model;

using System;
using System.Collections.Generic;

internal class DetailsComparer : IComparer<string>
{
    private static readonly string[] OrderedValues = new string[]
    {
        KnownDetails.Name,
        KnownDetails.DlcName,
        KnownDetails.ExtraTag,
        KnownDetails.Category,
        KnownDetails.Subcategory,
        KnownDetails.Update,
        KnownDetails.System,
        KnownDetails.Subsystem,
        KnownDetails.Region,
        KnownDetails.Version,
        KnownDetails.AppVersion,
        KnownDetails.Ps3SystemVersion,
        KnownDetails.Title,
        KnownDetails.TitleId,
        KnownDetails.ContentId,
        KnownDetails.Languages,
        KnownDetails.Keys,
        KnownDetails.Rap,
    };

    public int Compare(string? x, string? y)
    {
        return x == y ? 0 : x == null ? -1 : y == null ? -1 : Array.IndexOf(OrderedValues, x).CompareTo(Array.IndexOf(OrderedValues, y));
    }
}
