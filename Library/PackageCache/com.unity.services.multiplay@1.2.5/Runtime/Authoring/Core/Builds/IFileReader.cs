using System.Collections.Generic;
using System.IO;

namespace Unity.Services.Multiplay.Authoring.Core.Builds
{
    interface IFileReader
    {
        IEnumerable<string> EnumerateDirectories(string path);
        IEnumerable<string> EnumerateFiles(string path);
        Stream OpenReadFile(string path);

        bool DirectoryExists(string path);
    }
}
