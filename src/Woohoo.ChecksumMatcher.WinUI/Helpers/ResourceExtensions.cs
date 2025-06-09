// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Helpers;

using Microsoft.Windows.ApplicationModel.Resources;

public static class ResourceExtensions
{
    private static readonly ResourceLoader ResourceLoader = new();

    public static string GetLocalized(this string resourceKey) => ResourceLoader.GetString(resourceKey);
}
