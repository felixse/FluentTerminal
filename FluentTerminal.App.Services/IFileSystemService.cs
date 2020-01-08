using FluentTerminal.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IFileSystemService
    {
        Task<File> OpenFileAsync(IEnumerable<string> fileTypes);

        Task<string> BrowseForDirectoryAsync();

        Task SaveTextFileAsync(string name, string fileTypeDescription, string fileType, string content);

        Task<ImageFile> SaveImageInRoamingAsync(ImageFile imageFile);
    }
}