// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.WinUI.Pickers;

using Windows.Win32;
using Windows.Win32.Foundation;

internal static class PathHelper
{
    /// <summary>
    /// Retrieves the file system path for a known folder based on the specified location identifier.
    /// </summary>
    /// <param name="pickerLocationId">Specifies the location of the known folder to retrieve the path for.</param>
    /// <returns>Returns the path as a string, or an empty string if the path cannot be retrieved.</returns>
    public static string GetKnownFolderPath(Windows.Storage.Pickers.PickerLocationId pickerLocationId)
    {
        Guid folderId;

        switch (pickerLocationId)
        {
            case Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary:
                folderId = new Guid("FDD39AD0-238F-46AF-ADB4-6C85480369C7"); // FOLDERID_Documents
                break;
            case Windows.Storage.Pickers.PickerLocationId.ComputerFolder:
                folderId = new Guid("0AC0837C-BBF8-452A-850D-79D08E667CA7"); // FOLDERID_ComputerFolder
                break;
            case Windows.Storage.Pickers.PickerLocationId.Desktop:
                folderId = new Guid("B4BFCC3A-DB2C-424C-B029-7FE99A87C641"); // FOLDERID_Desktop
                break;
            case Windows.Storage.Pickers.PickerLocationId.Downloads:
                folderId = new Guid("374DE290-123F-4565-9164-39C4925E467B"); // FOLDERID_Downloads
                break;
            case Windows.Storage.Pickers.PickerLocationId.HomeGroup:
                folderId = new Guid("52528A6B-B9E3-4ADD-B60D-588C2DBA842D"); // FOLDERID_HomeGroup
                break;
            case Windows.Storage.Pickers.PickerLocationId.MusicLibrary:
                folderId = new Guid("4BD8D571-6D19-48D3-BE97-422220080E43"); // FOLDERID_MusicLibrary
                break;
            case Windows.Storage.Pickers.PickerLocationId.PicturesLibrary:
                folderId = new Guid("33E28130-4E1E-4676-835A-98395C3BC3BB"); // FOLDERID_PicturesLibrary
                break;
            case Windows.Storage.Pickers.PickerLocationId.VideosLibrary:
                folderId = new Guid("18989B1D-99B5-455B-841C-AB7C74E4DDFC"); // FOLDERID_VideosLibrary
                break;
            case Windows.Storage.Pickers.PickerLocationId.Objects3D:
                folderId = new Guid("31C0DD25-9439-4F12-BF41-7FF4EDA38722"); // FOLDERID_Objects3D
                break;
            case Windows.Storage.Pickers.PickerLocationId.Unspecified:
            default:
                return string.Empty;
        }

        PWSTR pszPath;
        int hr = PInvoke.SHGetKnownFolderPath(in folderId, 0, null, out pszPath);
        if (hr != 0)
        {
            return string.Empty;
        }

        return pszPath.ToString();
    }
}
