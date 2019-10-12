using FluentTerminal.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IFileSystemService
    {
        Task<File> OpenFile(IEnumerable<string> fileTypes);

        Task<string> BrowseForDirectory();

        Task SaveTextFile(string name, string fileTypeDescription, string fileType, string content);

        Task<ImageFile> ImportTemporaryImageFile(IEnumerable<string> fileTypes);

        Task<ImageFile> SaveBackgroundThemeImage(ImageFile imageFile);

        Task RemoveImportedImage(string fileName);
    }
}