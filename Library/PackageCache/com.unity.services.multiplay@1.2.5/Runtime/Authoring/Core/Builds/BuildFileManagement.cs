using System.Collections.Generic;
using System.Linq;

namespace Unity.Services.Multiplay.Authoring.Core.Builds
{
    class BuildFileManagement : IBuildFileManagement
    {
        const string k_BuildDirectory = "Builds/Multiplay/";

        IFileReader m_FileReader;

        public BuildFileManagement(IFileReader fileReader)
        {
            m_FileReader = fileReader;
        }

        public string DefaultBuildPath(string buildItemName)
        {
            return $"{k_BuildDirectory}{buildItemName}";
        }

        public List<BuildEntry> GetBuildEntriesAtPath(string buildPath)
        {
            var buildEntries = new List<BuildEntry>();
            foreach (var directory in m_FileReader.EnumerateDirectories(buildPath).Append(buildPath))
            {
                foreach (var filePath in m_FileReader.EnumerateFiles(directory))
                {
                    var fileStream = m_FileReader.OpenReadFile(filePath);
                    var uploadPath = filePath.Substring(buildPath.Length).Replace("\\", "/");
                    var buildEntry = new BuildEntry(uploadPath, fileStream);
                    buildEntries.Add(buildEntry);
                }
            }
            return buildEntries;
        }
    }
}
