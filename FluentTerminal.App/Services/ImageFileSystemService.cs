using FluentTerminal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace FluentTerminal.App.Services
{
    public class ImageFileSystemService : IImageFileSystemService
    {
        public async Task RemoveTemporaryBackgroundThemeImageAsync()
        {
            var folder = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync("BackgroundThemeTmp");

            if (folder == null)
            {
                return;
            }

            var backgroundThemeTmpFolder =
                await ApplicationData.Current.LocalCacheFolder.GetFolderAsync("BackgroundThemeTmp");

            if (backgroundThemeTmpFolder != null)
            {
                await backgroundThemeTmpFolder.DeleteAsync();
            }
        }

        public async Task<ImageFile> ImportTemporaryImageFileAsync(IEnumerable<string> fileTypes)
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

            if (file == null)
            {
                return null;
            }

            var backgroundThemeTmpFolder =
                await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("BackgroundThemeTmp",
                    CreationCollisionOption.OpenIfExists);

            if (backgroundThemeTmpFolder == null)
            {
                return null;
            }

            var item = await backgroundThemeTmpFolder.TryGetItemAsync(file.Name);

            if (item == null)
            {
                var storageFile = await file.CopyAsync(backgroundThemeTmpFolder, file.Name);

                return new ImageFile(storageFile.DisplayName, storageFile.FileType,
                    $@"{backgroundThemeTmpFolder.Path}\{storageFile.Name}");
            }

            return new ImageFile(file.DisplayName, file.FileType, $@"{backgroundThemeTmpFolder.Path}\{item.Name}");
        }

        public async Task RemoveImportedImageAsync(string fileName)
        {
            var backgroundThemeFolder =
                await ApplicationData.Current.RoamingFolder.CreateFolderAsync("BackgroundTheme",
                    CreationCollisionOption.OpenIfExists);

            if (backgroundThemeFolder == null)
            {
                return;
            }

            var item = await backgroundThemeFolder.TryGetItemAsync(fileName);

            if (item == null)
            {
                return;
            }

            var file = await backgroundThemeFolder.GetFileAsync(fileName);

            if (file != null)
            {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        public string EncodeImage(ImageFile imageFile)
        {
            if(imageFile == null)
            {
                return string.Empty;
            }

            if (!System.IO.File.Exists(imageFile.Path))
            {
                return string.Empty;
            }

            return Convert.ToBase64String(System.IO.File.ReadAllBytes(imageFile.Path));
        }

        public async Task<ImageFile> ImportThemeImageAsync(ImageFile backgroundImage, string encodedImage)
        {
            var bitmapData = Convert.FromBase64String(encodedImage);
            var streamBitmap = new MemoryStream(bitmapData);

            var localFolder =
                await ApplicationData.Current.RoamingFolder.CreateFolderAsync("BackgroundTheme",
                    CreationCollisionOption.OpenIfExists);

            var storageFile = await localFolder.CreateFileAsync($"{backgroundImage.Name}{backgroundImage.FileType}",
                CreationCollisionOption.GenerateUniqueName);

            using (var stream = await storageFile.OpenStreamForWriteAsync())
            {
                stream.Seek(0, SeekOrigin.Begin);
                streamBitmap.WriteTo(stream);
            }

            return new ImageFile(storageFile.DisplayName, storageFile.FileType, storageFile.Path);
        }
    }
}
