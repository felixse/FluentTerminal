using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FluentTerminal.Models
{
    public class ImageFile
    {
        public ImageFile(string name,
                         string fileType,
                         string path,
                         string base64String)
        {
            Name = name;
            FileType = fileType;
            Path = path;
            Base64String = base64String;
        }

        public string Name { get; }
        public string FileType { get; }
        public string Path { get; }
        public string Base64String { get; }
    }
}
