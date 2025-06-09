// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using System.Runtime.InteropServices;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Models;

public class SoundService : ISoundService
{
    public async Task PlayAsync(SystemSound sound, CancellationToken ct)
    {
        string? name = sound switch
        {
            SystemSound.Success => "SystemAsterisk",
            SystemSound.Error => "SystemHand",
            _ => null,
        };

        if (name is not null)
        {
            await Task.Run(() => NativeMethods.PlaySound(name, IntPtr.Zero, 0), ct);
        }
    }

    private static class NativeMethods
    {
        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern bool PlaySound(string lpszName, IntPtr hModule, int dwFlags);
    }
}
