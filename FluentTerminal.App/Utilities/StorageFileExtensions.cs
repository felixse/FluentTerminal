using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace FluentTerminal.App.Utilities
{
    public static class StorageFileExtensions
    {
        public static async Task<string> ImageToBase64String(this StorageFile storageFile)
        {
            var stream = await storageFile.OpenAsync(FileAccessMode.Read);

            var reader = new DataReader(stream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)stream.Size);
            byte[] byteArray = new byte[stream.Size];
            reader.ReadBytes(byteArray);

            return Convert.ToBase64String(byteArray);
        }
    }
}
