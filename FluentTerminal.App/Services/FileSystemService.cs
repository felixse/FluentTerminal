using Newtonsoft.Json;
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
