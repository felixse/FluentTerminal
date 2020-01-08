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
        public Task<string> BrowseForDirectoryAsync()
        {
            var picker = new FolderPicker{ SuggestedStartLocation = PickerLocationId.ComputerFolder };
            picker.FileTypeFilter.Add(".whatever"); // else a ComException is thrown

            return picker.PickSingleFolderAsync().AsTask()
                .ContinueWith(t => t.Result?.Path, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public async Task<File> OpenFileAsync(IEnumerable<string> fileTypes)
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

        public async Task<ImageFile> SaveImageInRoamingAsync(ImageFile imageFile)
        {
            var file = await StorageFile.GetFileFromPathAsync(imageFile.Path);

            var backgroundThemeFolder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("BackgroundTheme", CreationCollisionOption.OpenIfExists);

            var storageFile = await file.CopyAsync(backgroundThemeFolder, file.DisplayName, NameCollisionOption.GenerateUniqueName);

            return new ImageFile(
                storageFile.DisplayName,
                storageFile.FileType,
                storageFile.Path);
        }

        public async Task SaveTextFileAsync(string name, string fileTypeDescription, string fileType, string content)
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder, 
                SuggestedFileName = name
            };
            picker.FileTypeChoices.Add(fileTypeDescription, new List<string> { fileType });

            var file = await picker.PickSaveFileAsync();

            if (file != null)
            {
                await FileIO.WriteTextAsync(file, content);
            }
        }
    }
}