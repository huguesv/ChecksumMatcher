// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.WinUI.Pickers;

using Windows.Win32;
using Windows.Win32.UI.Shell;

internal static partial class PickerHelper
{
    private static readonly Guid ShellItemGuid = typeof(IShellItem).GUID;

    internal static Windows.Win32.UI.Shell.FILEOPENDIALOGOPTIONS MapPickerOptionsToFOS(PickerOptions options)
    {
        return (Windows.Win32.UI.Shell.FILEOPENDIALOGOPTIONS)options;
    }

    internal static unsafe void SetInitialDirectory(IFileDialog* dialog, string initialFolder)
    {
        fixed (char* initialDir = initialFolder)
        {
            fixed (Guid* itemGuid = &PickerHelper.ShellItemGuid)
            {
                void* shellItem = null;
                PInvoke.SHCreateItemFromParsingName(initialDir, null, itemGuid, &shellItem);
                var psi = (IShellItem*)shellItem;

                dialog->SetFolder(psi);
            }
        }
    }

    internal static unsafe void SetInitialDirectory(IFileOpenDialog* dialog, string initialFolder)
    {
        fixed (char* initialDir = initialFolder)
        {
            fixed (Guid* itemGuid = &PickerHelper.ShellItemGuid)
            {
                void* shellItem = null;
                PInvoke.SHCreateItemFromParsingName(initialDir, null, itemGuid, &shellItem);
                var psi = (IShellItem*)shellItem;

                dialog->SetFolder(psi);
            }
        }
    }

    internal static unsafe List<string> GetFileOpenResults(IFileOpenDialog* dialog, bool allowMultiple)
    {
        var filePaths = new List<string>();

        if (allowMultiple)
        {
            IShellItemArray* resultsPtr = null;
            dialog->GetResults(&resultsPtr);

            if (resultsPtr != null)
            {
                resultsPtr->GetCount(out var count);

                for (uint i = 0; i < count; i++)
                {
                    IShellItem* itemPtr = null;
                    resultsPtr->GetItemAt(i, &itemPtr);
                    if (itemPtr != null)
                    {
                        itemPtr->GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out var filePath);
                        if (filePath != null)
                        {
                            filePaths.Add(filePath.ToString());
                        }
                    }
                }
            }
        }
        else
        {
            IShellItem* resultsPtr = null;
            dialog->GetResult(&resultsPtr);

            if (resultsPtr != null)
            {
                resultsPtr->GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out var filePath);
                if (filePath != null)
                {
                    filePaths.Add(filePath.ToString());
                }
            }
        }

        return filePaths;
    }
}
