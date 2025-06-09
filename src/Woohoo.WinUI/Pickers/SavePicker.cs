// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.WinUI.Pickers;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Storage;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.Shell.Common;
using WinRT.Interop;

[SupportedOSPlatform("windows")]
public sealed class SavePicker
{
    private readonly nint hwnd;

    public SavePicker(nint hwnd)
    {
        this.hwnd = hwnd;
    }

    public SavePicker(Microsoft.UI.Xaml.Window window)
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
    /// Prompts the user to select a file to save and returns the selected file path as a string.
    /// </summary>
    /// <returns>Returns the path of the selected file or null if no file was selected.</returns>
    public string? PickSaveFile()
    {
        return this.SaveFileDialog();
    }

    /// <summary>
    /// Asynchronously prompts the user to select a location to save a file and returns the corresponding storage file.
    /// </summary>
    /// <returns>Returns the selected storage file or null if no file was chosen.</returns>
    public async Task<StorageFile?> PickSaveFileAsync()
    {
        var filePath = this.SaveFileDialog();
        return filePath != null ? await this.GetStorageFileOrCreateAsync(filePath) : null;
    }

    private async Task<StorageFile> GetStorageFileOrCreateAsync(string filePath)
    {
        if (File.Exists(filePath))
        {
            return await StorageFile.GetFileFromPathAsync(filePath);
        }
        else
        {
            var folder = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);
            var storageFolder = await StorageFolder.GetFolderFromPathAsync(folder);

            var storageFile = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await storageFile.DeleteAsync();
            return storageFile;
        }
    }

    private unsafe string? SaveFileDialog()
    {
        int hr = Windows.Win32.PInvoke.CoCreateInstance<IFileSaveDialog>(
                                    typeof(FileSaveDialog).GUID,
                                    null,
                                    CLSCTX.CLSCTX_INPROC_SERVER,
                                    out var fsd);
        if (hr < 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        var dialogPtr = (nint)fsd;
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

            dialog->SetOptions(PickerHelper.MapPickerOptionsToFOS(this.Options));

            try
            {
                dialog->Show(new HWND(this.hwnd));
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x800704C7)
            {
                // User canceled the dialog, return null
                return null;
            }

            IShellItem* resultsPtr = null;
            dialog->GetResult(&resultsPtr);
            if (resultsPtr != null)
            {
                resultsPtr->GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out var filePath);
                return filePath.ToString();
            }

            return null;
        }
        finally
        {
            dialog->Close(new HRESULT(0));
            dialog->Release();
        }
    }
}
