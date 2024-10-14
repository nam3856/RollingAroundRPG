using System.Collections.Generic;
using System.IO;

namespace Unity.Services.Multiplay.Authoring.Core.Builds
{
    class FileReader : IFileReader
    {
        private string k_AllFilesSearchPattern = "*.*";

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public IEnumerable<string> EnumerateDirectories(string path)
        {
            return Directory.EnumerateDirectories(path, k_AllFilesSearchPattern, SearchOption.AllDirectories);
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            return Directory.EnumerateFiles(path);
        }

        public Stream OpenReadFile(string path)
        {
            return File.OpenRead(path);
        }
    }
}
