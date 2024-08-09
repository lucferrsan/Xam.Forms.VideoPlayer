using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Xam.Forms.VideoPlayer.UWP;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

[assembly: Dependency(typeof(VideoPicker))]

namespace Xam.Forms.VideoPlayer.UWP
{
    public class VideoPicker : IVideoPicker
    {
        public async Task<string> GetVideoFileAsync()
        {
/*
    TODO You should replace 'App.WindowHandle' with the your window's handle (HWND) 
    Read more on retrieving window handle here: https://docs.microsoft.com/en-us/windows/apps/develop/ui-input/retrieve-hwnd
*/
            // Create and initialize the FileOpenPicker
            FileOpenPicker openPicker = InitializeWithWindow(new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.VideosLibrary
            },App.WindowHandle);

            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".mkv");
            openPicker.FileTypeFilter.Add(".wmv");

            // Get a file and return the path 
            StorageFile storageFile = await openPicker.PickSingleFileAsync();
            return storageFile?.Path;
        }

        private static FileOpenPicker InitializeWithWindow(FileOpenPicker obj, IntPtr windowHandle)
        {
            WinRT.Interop.InitializeWithWindow.Initialize(obj, windowHandle);
            return obj;
        }
    }
}
