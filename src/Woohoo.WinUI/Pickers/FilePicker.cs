// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.WinUI.Pickers;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Storage;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.Shell.Common;
using WinRT.Interop;

[SupportedOSPlatform("windows")]
public sealed class FilePicker
{
    private readonly nint hwnd;

    public FilePicker(nint hwnd)
    {
        this.hwnd = hwnd;
    }

    public FilePicker(Microsoft.UI.Xaml.Window window)
        : this(WindowNative.GetWindowHandle(window))
    {
    }

    public PickerOptions Options { get; set; } = PickerOptions.None;

    public bool ShowDetailedExtension { get; set; } = true;

    public string? CommitButtonText { get; set; }

    public string? SuggestedFileName { get; set; }

    public string? DefaultFileExtension { get; set; }

    public string? InitialDirectory { get; set; }

    public Windows.Storage.Pickers.PickerLocationId SuggestedStartLocation { get; set; } = Windows.Storage.Pickers.PickerLocationId.Unspecified;

    public string? Title { get; set; }

    public Dictionary<string, IList<string>> FileTypeChoices { get; set; } = new();

    public bool ShowAllFilesOption { get; set; } = true;

    /// <summary>
    /// picks a single file.
    /// </summary>
    /// <returns>Returns the path of the selected file or null if no file was selected.</returns>
    public string? PickSingleFile()
    {
        var files = this.OpenFileDialog(false);
        return files.Count > 0 ? files[0] : null;
    }

    /// <summary>
    /// Asynchronously picks single file.
    /// </summary>
    /// <returns>Returns the selected file as a StorageFile or null if no file was selected.</returns>
    public async Task<StorageFile?> PickSingleFileAsync()
    {
        var files = this.OpenFileDialog(false);
        return files.Count > 0 ? await StorageFile.GetFileFromPathAsync(files[0]) : null;
    }

    /// <summary>
    /// picks multiple files.
    /// </summary>
    /// <returns>Returns the path of the selected files or null if no file was selected.</returns>
    public List<string> PickMultipleFiles()
    {
        return this.OpenFileDialog(true);
    }

    /// <summary>
    /// Asynchronously picks multiple files.
    /// </summary>
    /// <returns>Returns A list of StorageFile selected by the user.</returns>
    public async Task<List<StorageFile>> PickMultipleFilesAsync()
    {
        var filePaths = this.OpenFileDialog(true);
        var storageFiles = new List<StorageFile>();
        foreach (var path in filePaths)
        {
            storageFiles.Add(await StorageFile.GetFileFromPathAsync(path));
        }

        return storageFiles;
    }

    private unsafe List<string> OpenFileDialog(bool allowMultiple)
    {
        int hr = PInvoke.CoCreateInstance<IFileOpenDialog>(
                            typeof(FileOpenDialog).GUID,
                            null,
                            CLSCTX.CLSCTX_INPROC_SERVER,
                            out var fod);
        if (hr < 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        var dialogPtr = (nint)fod;
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
                PInvoke.SHCreateItemFromParsingName(this.InitialDirectory, null, typeof(IShellItem).GUID, out var ppv);
                var psi = (IShellItem*)ppv;

                dialog->SetFolder(psi);
            }

            if (!string.IsNullOrEmpty(this.SuggestedFileName))
            {
                dialog->SetFileName(this.SuggestedFileName);
            }

            var filters = new List<COMDLG_FILTERSPEC>();

            if (this.ShowAllFilesOption)
            {
                filters.Add(new COMDLG_FILTERSPEC { pszName = (char*)Marshal.StringToHGlobalUni("All Files (*.*)"), pszSpec = (char*)Marshal.StringToHGlobalUni("*.*") });
            }

            foreach (var kvp in this.FileTypeChoices)
            {
                var displayName = kvp.Key;

                if (this.ShowDetailedExtension)
                {
                    var extensions = string.Join(", ", kvp.Value);
                    displayName = $"{kvp.Key} ({extensions})";
                }

                var spec = string.Join(";", kvp.Value);
                filters.Add(new COMDLG_FILTERSPEC { pszName = (char*)Marshal.StringToHGlobalUni(displayName), pszSpec = (char*)Marshal.StringToHGlobalUni(spec) });
            }

            dialog->SetFileTypes(filters.ToArray());

            if (!string.IsNullOrEmpty(this.DefaultFileExtension) && this.FileTypeChoices.ContainsKey(this.DefaultFileExtension))
            {
                var defaultIndex = new List<string>(this.FileTypeChoices.Keys).IndexOf(this.DefaultFileExtension) + (this.ShowAllFilesOption ? 1 : 0);
                dialog->SetFileTypeIndex((uint)(defaultIndex + 1));
            }

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
        finally
        {
            dialog->Close(new HRESULT(0));
            dialog->Release();
        }
    }
}
