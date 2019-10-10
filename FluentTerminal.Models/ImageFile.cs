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
                         string path)
        {
            Name = name;
            FileType = fileType;
            Path = path;
        }

        public string Name { get; }
        public string FileType { get; }
        public string Path { get; }
    }
}
