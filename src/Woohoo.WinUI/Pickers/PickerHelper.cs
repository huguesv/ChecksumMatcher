// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.WinUI.Pickers;

internal static partial class PickerHelper
{
    internal static Windows.Win32.UI.Shell.FILEOPENDIALOGOPTIONS MapPickerOptionsToFOS(PickerOptions options)
    {
        return (Windows.Win32.UI.Shell.FILEOPENDIALOGOPTIONS)options;
    }
}
