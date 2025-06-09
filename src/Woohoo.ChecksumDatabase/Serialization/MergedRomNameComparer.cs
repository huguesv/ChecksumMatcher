// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using System.Collections.Generic;

public class MergedRomNameComparer : IComparer<MergedRom>
{
    public static MergedRomNameComparer Instance { get; } = new MergedRomNameComparer();

    public int Compare(MergedRom? x, MergedRom? y)
    {
        if (x is not null && y is not null)
        {
            var name1 = string.IsNullOrEmpty(x.OriginalParentGameName) ? x.Name : x.OriginalParentGameName + "\\" + x.Name;
            var name2 = string.IsNullOrEmpty(y.OriginalParentGameName) ? y.Name : y.OriginalParentGameName + "\\" + y.Name;

            return Compare(name1, name2);
        }
        else if (x is null)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

    private static int Compare(string s1, string s2)
    {
        bool ns1 = s1.Contains('\\');
        bool ns2 = s2.Contains('\\');

        if (ns1 && !ns2)
        {
            return 1;
        }

        if (ns2 && !ns1)
        {
            return -1;
        }

        if (ns1 && ns2)
        {
            string ts1 = s1[..s1.IndexOf('\\')];
            string ts2 = s2[..s2.IndexOf('\\')];
            if (ts1 == ts2)
            {
                ts1 = s1[(s1.IndexOf('\\') + 1)..];
                ts2 = s2[(s2.IndexOf('\\') + 1)..];
            }

            s1 = ts1;
            s2 = ts2;
        }

        int len1 = s1.Length;
        int len2 = s2.Length;
        int marker1 = 0;
        int marker2 = 0;

        // Walk through two the strings with two markers.
        while (marker1 < len1 && marker2 < len2)
        {
            char ch1 = s1[marker1];
            char ch2 = s2[marker2];

            // Some buffers we can build up characters in for each chunk.
            char[] space1 = new char[len1];
            int loc1 = 0;
            char[] space2 = new char[len2];
            int loc2 = 0;

            // Walk through all following characters that are digits or
            // characters in BOTH strings starting at the appropriate marker.
            // Collect char arrays.
            do
            {
                space1[loc1++] = ch1;
                marker1++;

                if (marker1 < len1)
                {
                    ch1 = s1[marker1];
                }
                else
                {
                    break;
                }
            }
            while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

            do
            {
                space2[loc2++] = ch2;
                marker2++;

                if (marker2 < len2)
                {
                    ch2 = s2[marker2];
                }
                else
                {
                    break;
                }
            }
            while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

            // If we have collected numbers, compare them numerically.
            // Otherwise, if we have strings, compare them alphabetically.
            string str1 = new string(space1, 0, loc1);
            string str2 = new string(space2, 0, loc2);

            int result;

            if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
            {
                long thisNumericChunk = long.Parse(str1);
                long thatNumericChunk = long.Parse(str2);
                result = thisNumericChunk.CompareTo(thatNumericChunk);
                if (result == 0)
                {
                    if (loc1 != loc2)
                    {
                        return (loc1 - loc2) < 0 ? -1 : 1;
                    }
                }
            }
            else
            {
                result = string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase);
            }

            if (result != 0)
            {
                return result < 0 ? -1 : 1;
            }
        }

        int lenDiff = len2 - len1;
        return lenDiff == 0 ? 0 : (lenDiff < 0 ? -1 : 1);
    }
}
