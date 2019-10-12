using FluentTerminal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentTerminal.App.Services
{
    public class ImageFileSystemService : IImageFileSystemService
    {
        public async Task RemoveTemporaryBackgroundThemeImage()
        {
            var folder = await ApplicationData.Current
                                     .LocalCacheFolder
                                     .TryGetItemAsync("BackgroundThemeTmp");

            if (folder == null)
            {
                return;
            }

            var backgroundThemeTmpFolder =
                await ApplicationData.Current
                                     .LocalCacheFolder
                                     .GetFolderAsync(
                                            "BackgroundThemeTmp");

            await backgroundThemeTmpFolder.DeleteAsync();
        }

        public async Task<ImageFile> ImportTemporaryImageFile(IEnumerable<string> fileTypes)
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            foreach (var fileType in fileTypes)
            {
                picker.FileTypeFilter.Add(fileType);
            }

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                var backgroundThemeTmpFolder =
                    await ApplicationData.Current
                                         .LocalCacheFolder
                                         .CreateFolderAsync(
                                                "BackgroundThemeTmp",
                                                CreationCollisionOption.OpenIfExists);

                var item = await backgroundThemeTmpFolder.TryGetItemAsync(file.Name);

                if (item == null)
                {
                    var storageFile = await file.CopyAsync(backgroundThemeTmpFolder, file.Name);

                    return new ImageFile(
                        storageFile.DisplayName,
                        storageFile.FileType,
                        $@"{backgroundThemeTmpFolder.Path}\{storageFile.Name}");
                }

                return new ImageFile(
                    file.DisplayName,
                    file.FileType,
                    $@"{backgroundThemeTmpFolder.Path}\{item.Name}");
            }

            return null;
        }

        public async Task RemoveImportedImage(string fileName)
        {
            var backgroundThemeFolder = await ApplicationData.Current.RoamingFolder
                .CreateFolderAsync("BackgroundTheme", CreationCollisionOption.OpenIfExists);

            var item = await backgroundThemeFolder.TryGetItemAsync(fileName);

            if (item != null)
            {
                var file = await backgroundThemeFolder.GetFileAsync(fileName);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        public string EncodeImage(ImageFile imageFile)
        {
            return Convert.ToBase64String(System.IO.File.ReadAllBytes(imageFile.Path));
        }

        public async Task ImportThemeImage(ImageFile backgroundImage, string encodedImage)
        {
            var bitmapData = Convert.FromBase64String(encodedImage);
            MemoryStream streamBitmap = new MemoryStream(bitmapData);

            var localFolder = await ApplicationData.Current
                                                   .RoamingFolder
                                                   .CreateFolderAsync("BackgroundTheme", CreationCollisionOption.OpenIfExists);

            var storageFile = await localFolder
                            .CreateFileAsync(
                                $"{backgroundImage.Name}{backgroundImage.FileType}", 
                                CreationCollisionOption.GenerateUniqueName);

            using (Stream stream = await storageFile.OpenStreamForWriteAsync())
            {
                stream.Seek(0, SeekOrigin.Begin);
                streamBitmap.WriteTo(stream);
            }
        }
    }
}
