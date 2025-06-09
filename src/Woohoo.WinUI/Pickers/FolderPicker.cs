// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.WinUI.Pickers;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Storage;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;
using WinRT.Interop;

[SupportedOSPlatform("windows")]
public sealed class FolderPicker
{
    private readonly nint hwnd;

    public FolderPicker(nint hwnd)
    {
        this.hwnd = hwnd;
    }

    public FolderPicker(Microsoft.UI.Xaml.Window window)
        : this(WindowNative.GetWindowHandle(window))
    {
    }

    public PickerOptions Options { get; set; } = PickerOptions.None;

    public string? CommitButtonText { get; set; }

    public string? SuggestedFileName { get; set; }

    public string? InitialDirectory { get; set; }

    public Windows.Storage.Pickers.PickerLocationId SuggestedStartLocation { get; set; } = Windows.Storage.Pickers.PickerLocationId.Unspecified;

    public string? Title { get; set; }

    /// <summary>
    /// Picks a single folder.
    /// </summary>
    /// <returns>Returns the path of the selected folder or null if no folder was selected.</returns>
    public string? PickSingleFolder()
    {
        var folderPaths = this.OpenFolderDialog(false);
        return folderPaths.Count > 0 ? folderPaths[0] : null;
    }

    /// <summary>
    /// Asynchronously picks a single folder.
    /// </summary>
    /// <returns>Returns the selected folder as a StorageFolder or null if no folder was selected.</returns>
    public async Task<StorageFolder?> PickSingleFolderAsync()
    {
        var folderPaths = this.OpenFolderDialog(false);
        return folderPaths.Count > 0 ? await StorageFolder.GetFolderFromPathAsync(folderPaths[0]) : null;
    }

    /// <summary>
    /// Picks multiple folders.
    /// </summary>
    /// <returns>Returns the path of the selected folders or null if no folder was selected.</returns>
    public List<string> PickMultipleFolders()
    {
        return this.OpenFolderDialog(true);
    }

    /// <summary>
    /// Asynchronously picks multiple folders.
    /// </summary>
    /// <returns>Returns A list of StorageFolder selected by the user.</returns>
    public async Task<List<StorageFolder>> PickMultipleFoldersAsync()
    {
        var folderPaths = this.OpenFolderDialog(true);
        var storageFolders = new List<StorageFolder>();
        foreach (var path in folderPaths)
        {
            storageFolders.Add(await StorageFolder.GetFolderFromPathAsync(path));
        }

        return storageFolders;
    }

    private unsafe List<string> OpenFolderDialog(bool allowMultiple)
    {
        int hr = Windows.Win32.PInvoke.CoCreateInstance<IFileOpenDialog>(
                            typeof(FileOpenDialog).GUID,
                            null,
                            CLSCTX.CLSCTX_INPROC_SERVER,
                            out var fpd);
        if (hr < 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        var dialogPtr = (nint)fpd;
        var dialog = (IFileOpenDialog*)dialogPtr;

        try
        {
            if (!string.IsNullOrEmpty(this.Title))
            {
                dialog->SetTitle(this.Title);
            }

            if (!string.IsNullOrEmpty(this.CommitButtonText))
            {
                dialog->SetOkButtonLabel(this.CommitButtonText);
            }

            if (this.SuggestedStartLocation != Windows.Storage.Pickers.PickerLocationId.Unspecified)
            {
                this.InitialDirectory = PathHelper.GetKnownFolderPath(this.SuggestedStartLocation);
            }

            if (!string.IsNullOrEmpty(this.InitialDirectory))
            {
                Windows.Win32.PInvoke.SHCreateItemFromParsingName(this.InitialDirectory, null, typeof(IShellItem).GUID, out var ppv);
                var psi = (IShellItem*)ppv;

                dialog->SetFolder(psi);
            }

            if (!string.IsNullOrEmpty(this.SuggestedFileName))
            {
                dialog->SetFileName(this.SuggestedFileName);
            }

            this.Options |= PickerOptions.FOS_PICKFOLDERS;

            if (allowMultiple)
            {
                this.Options |= PickerOptions.FOS_ALLOWMULTISELECT;
            }

            dialog->SetOptions(PickerHelper.MapPickerOptionsToFOS(this.Options));

            try
            {
                dialog->Show(new HWND(this.hwnd));
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x800704C7)
            {
                // User canceled the dialog, return an empty list
                return new List<string>();
            }

            var folderPaths = new List<string>();

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
                                folderPaths.Add(filePath.ToString());
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
                        folderPaths.Add(filePath.ToString());
                    }
                }
            }

            return folderPaths;
        }
        finally
        {
            dialog->Close(new HRESULT(0));
            dialog->Release();
        }
    }
}
