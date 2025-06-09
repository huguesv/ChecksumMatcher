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
    public static void NotNull<T>([ValidatedNotNull, NotNull] T? value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    [DebuggerStepThrough]
    public static void NotNullOrEmpty([ValidatedNotNull, NotNull] string? value, [CallerArgumentExpression("value")] string? paramName = null)
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
    public static void NotNullOrWhitespace([ValidatedNotNull, NotNull] string? value, [CallerArgumentExpression("value")] string? paramName = null)
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
