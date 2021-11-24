using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SE.AppConfiguration.Core
{
    internal static class SatelliteResolver
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        static Assembly LoadFromAppConfigFolder(object sender, ResolveEventArgs args)
        {
            log.Debug($"SatelliteResolver: RegisterResolveEventHandler(): Requested Assembly {args.Name}");

            string assemblyPath = Path.Combine(searchFolder, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath))
            {
                return null;
            }

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyPath);

                if (assembly != null)
                {
                    return assembly;
                }
            }
            catch (FileLoadException flex)
            {
                log.Error(flex, $"The Assembly [{assemblyPath}] cannot be loaded.");

                if ((uint)flex.HResult == 0x80131515)
                {
                    log.Error(flex, $"The assembly appears to be blocked.\n");

                    if (flex.InnerException != null)
                    {
                        log.Error(flex, $"{flex.InnerException.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, $"Trying to locate assembly: {args.Name}");
            }

            log.Debug($"Failed to find assembly for: {args.Name}");
            return null;
        }

        static public void RegisterResolveEventHandler()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromAppConfigFolder);
            log.Debug($"SatelliteResolver: RegisterResolveEventHandler()");
        }

        static public void UnregisterResolveEventHandler()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve -= new ResolveEventHandler(LoadFromAppConfigFolder);
            log.Debug($"SatelliteResolver: UnregisterResolveEventHandler()");
        }

        static public void SetAppConfigPath(string filePath)
        {
            searchFolder = System.IO.Path.GetDirectoryName(filePath);
            log.Debug($"SatelliteResolver: Setting search path to [{searchFolder}]");
        }

        static string searchFolder;
    }
    public class FileSystem : IFileSystem
    {
        readonly Logger log = LogManager.GetCurrentClassLogger();

        public List<string> FindFiles(string Folder, string FileName)
        {
            return new DirectoryInfo(Folder)
                         .EnumerateFiles(FileName, SearchOption.AllDirectories)
                         .Select(d => d.FullName).ToList();
        }

        public bool FileExists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public void CopyFile(string Source, string Destination)
        {
            System.IO.File.Copy(Source, Destination);
        }

        public string LoadFile(string appConfigFilePath)
        {
            return File.ReadAllText(appConfigFilePath);
        }

        public void DeleteFile(string path)
        {
            System.IO.File.Delete(path);
        }

        public Configuration LoadAppConfigFile(string path)
        {
            // Helpfully ConfigurationManager.OpenExeConfiguration expects the name of the executable here, not he actuall name of the app.config file.
            // const string CONFIG_EXTENSION = ".config";
            //if ( path.EndsWith(CONFIG_EXTENSION))
            //{
            //    path = path.Remove(path.Length - CONFIG_EXTENSION.Length);
            //}

            try
            {
                SatelliteResolver.SetAppConfigPath(path);

                log.Debug($"Loading configuration for executable: [{path}]");
                var cfg = ConfigurationManager.OpenExeConfiguration(path);

                if (cfg.HasFile == false)
                {
                    var extension = System.IO.Path.GetExtension(path);

                    log.Error("The configuration file has not been located, This operation is unlikely to work.");

                    if (string.Compare(extension, ".config") == 0)
                    {
                        log.Error("THE PATH MUST BE TO THE EXECUTABLE FILE (NOT the *.exe.config file).");
                    }
                }

                log.Debug("successfully loaded: {0}", cfg.FilePath);
                return cfg;
            }
            catch (Exception e)
            {
                var message = string.Format("Failed to load app.config file [{0}]", path);
                log.Fatal(e, message);
                throw new SecureAppConfigException(message, e);
            }
        }

        private string AppendTimeStampToFileName(string filePath)
        {
            var folderName = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);

            var extensionsstack = new Stack<string>();
            string extension;

            // deal with files that have multiple extensions like *.exe.config files
            do
            {
                extension = Path.GetExtension(fileName);

                if (string.IsNullOrEmpty(extension))
                {
                    break;
                }

                fileName = fileName.Remove(fileName.Length - extension.Length);
                extensionsstack.Push(extension);
            }
            while (!string.IsNullOrEmpty(extension));

            fileName = string.Format("{0}_{1}", fileName, DateTime.UtcNow.ToString("yyyy-MM-dd--HH-mm-ss"));

            // put the extensions back on
            while (extensionsstack.Count > 0)
            {
                fileName += extensionsstack.Pop();
            }

            return Path.Combine(folderName, fileName) + extension;
        }

        public string BackupFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                log.Warn("Could not backup [{0}] file not found.", filePath);
                return string.Empty;
            }

            var copyName = AppendTimeStampToFileName(filePath);

            try
            {
                log.Info("Backing up file [{0}] to [{1}] ...", filePath, copyName);
                File.Copy(filePath, copyName, true);

                if (File.Exists(copyName))
                {
                    log.Info("confirmed backup file created: [{0}]", copyName);
                    return copyName;
                }
                else
                {
                    log.Error("Backup file NOT found: [{0}]", copyName);
                    return string.Empty;
                }
            }
            catch (Exception e)
            {
                var message = string.Format("Failed to backup file [{0}] to [{1}]", filePath, copyName);
                log.Fatal(e, message);
                throw new SecureAppConfigException(message, e);
            }
        }

        public bool IsPathValid(string path)
        {
            System.IO.FileInfo fi = null;
            try
            {
                fi = new System.IO.FileInfo(path);
            }
            catch (ArgumentException) { }
            catch (System.IO.PathTooLongException) { }
            catch (NotSupportedException) { }
            if (ReferenceEquals(fi, null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
