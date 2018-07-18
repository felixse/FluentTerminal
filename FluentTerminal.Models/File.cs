using System.IO;

namespace FluentTerminal.Models
{
    public class File
    {
        public File(string name, string fileType, string path, Stream content)
        {
            Name = name;
            FileType = fileType;
            Path = path;
            Content = content;
        }

        public string Name { get; }
        public string FileType { get; }
        public string Path { get; }
        public Stream Content { get; }
    }
}