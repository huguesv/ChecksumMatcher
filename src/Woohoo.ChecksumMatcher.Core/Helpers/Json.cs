// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Helpers;

using System.Text.Json;

public static class Json
{
    public static async Task<T?> ToObjectAsync<T>(string value)
    {
        return await Task.Run<T?>(() =>
        {
            return JsonSerializer.Deserialize<T>(value);
        });
    }

    public static async Task<string> StringifyAsync(object? value)
    {
        return await Task.Run<string>(() =>
        {
            return JsonSerializer.Serialize(value);
        });
    }

    public static T? ToObject<T>(string value)
    {
        return JsonSerializer.Deserialize<T>(value);
    }

    public static string Stringify(object? value)
    {
        return JsonSerializer.Serialize(value);
    }
}
