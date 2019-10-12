using FluentTerminal.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface IImageFileSystemService
    {
        Task<ImageFile> ImportTemporaryImageFile(IEnumerable<string> fileTypes);

        Task RemoveTemporaryBackgroundThemeImage();

        Task RemoveImportedImage(string fileName);
    }
}
