using System.Collections.Generic;

namespace SE.AppConfiguration
{
    public interface IFileSystem
    {
        List<string> FindFiles(string Folder, string FileName);
        bool FileExists(string path);
        void CopyFile(string Source, string Destination);
        string LoadFile(string appConfigFilePath);
        void DeleteFile(string path);

        System.Configuration.Configuration LoadAppConfigFile(string path);
        string BackupFile(string filePath);

        bool IsPathValid(string path);
    }
}
