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

        public override bool Equals(object obj)
        {
            return obj is ImageFile file &&
                   Name == file.Name &&
                   FileType == file.FileType &&
                   Path == file.Path;
        }

        public override int GetHashCode()
        {
            var hashCode = -596750737;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            return hashCode;
        }
    }
}
