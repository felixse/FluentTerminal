using FluentTerminal.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IImageFileSystemService
    {
        Task<ImageFile> ImportTemporaryImageFileAsync(IEnumerable<string> fileTypes);

        Task RemoveTemporaryBackgroundThemeImageAsync();

        Task RemoveImportedImageAsync(string fileName);

        string EncodeImage(ImageFile imageFile);

        Task<ImageFile> ImportThemeImageAsync(ImageFile backgroundImage, string encodedImage);
    }
}
