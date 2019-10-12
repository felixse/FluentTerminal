using FluentTerminal.App.Utilities;
using FluentTerminal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using File = FluentTerminal.Models.File;

namespace FluentTerminal.App.Services
{
    public class FileSystemService : IFileSystemService
    {
        public async Task<string> BrowseForDirectory()
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add(".whatever"); // else a ComException is thrown
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            var folder = await picker.PickSingleFolderAsync();

            return folder?.Path;
        }

        public async Task<File> OpenFile(IEnumerable<string> fileTypes)
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            foreach (var fileType in fileTypes)
            {
                picker.FileTypeFilter.Add(fileType);
            }

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var stream = await file.OpenStreamForReadAsync().ConfigureAwait(true);

                return new File(file.DisplayName, file.FileType, file.Path, stream);
            }
            return null;
        }

        public async Task<ImageFile> SaveBackgroundThemeImage(ImageFile imageFile)
        {
            var file = await StorageFile.GetFileFromPathAsync(imageFile.Path);

            var backgroundThemeFolder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("BackgroundTheme", CreationCollisionOption.OpenIfExists);

            var storageFile = await file.CopyAsync(backgroundThemeFolder, imageFile.Name, NameCollisionOption.GenerateUniqueName);

            return new ImageFile(
                storageFile.DisplayName,
                storageFile.FileType,
                storageFile.Path);
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
            var item = await ApplicationData.Current.RoamingFolder.TryGetItemAsync(fileName);

            if (item != null)
            {
                var file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileName);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        public async Task SaveTextFile(string name, string fileTypeDescription, string fileType, string content)
        {
            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.SuggestedFileName = name;
            picker.FileTypeChoices.Add(fileTypeDescription, new List<string> { fileType });

            var file = await picker.PickSaveFileAsync();

            if (file != null)
            {
                await FileIO.WriteTextAsync(file, content);
            }
        }
    }
}