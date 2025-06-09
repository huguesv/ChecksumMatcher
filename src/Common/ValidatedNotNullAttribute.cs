// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo;

using System;

/// <summary>
/// Indicates to Code Analysis that a method validates a particular parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class ValidatedNotNullAttribute : Attribute
{
}
