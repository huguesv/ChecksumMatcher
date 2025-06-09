// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

#nullable enable

namespace Woohoo;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal static class Requires
{
    [DebuggerStepThrough]
#if NETSTANDARD2_0
    public static void NotNull<T>(T? value, string? paramName = null)
#else
    public static void NotNull<T>([ValidatedNotNull, NotNull] T? value, [CallerArgumentExpression("value")] string? paramName = null)
#endif
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    [DebuggerStepThrough]
#if NETSTANDARD2_0
    public static void NotNullOrEmpty(string? value, string? paramName = null)
#else
    public static void NotNullOrEmpty([ValidatedNotNull, NotNull] string? value, [CallerArgumentExpression("value")] string? paramName = null)
#endif
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        if (value.Length == 0)
        {
            throw new ArgumentException("Parameter value should not be empty string.", paramName);
        }
    }

    [DebuggerStepThrough]
#if NETSTANDARD2_0
    public static void NotNullOrWhitespace(string? value, string? paramName = null)
#else
    public static void NotNullOrWhitespace([ValidatedNotNull, NotNull] string? value, [CallerArgumentExpression("value")] string? paramName = null)
#endif
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        if (value.Length == 0)
        {
            throw new ArgumentException("Parameter value should not be empty string.", paramName);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Parameter value should not be white space.", paramName);
        }
    }
}
