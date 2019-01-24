using System;
using System.IO;
using System.Reflection;

namespace My.XMLUtilities
{
    public static class ApplicationValue
    {

        private static bool useCommonAppDataFolder = true;

        public static string ApplicationName { get { return EntryExecFilename; } }
        public static string BackupFolderName { get { return ApplicationName + ".Backup"; } }
        public static bool UseCommonAppDataFolder { get { return useCommonAppDataFolder; } set { useCommonAppDataFolder = value; } }
        public static Version ApplicationVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        private static string EntryExecFilename
        {
            get
            {
                string location = Assembly.GetEntryAssembly().Location;
                return Path.GetFileNameWithoutExtension(location);
            }
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string EntryExecPath
        {
            get
            {
                string location = Assembly.GetEntryAssembly().Location;
                return Path.GetDirectoryName(location);
            }
        }

        public static string ExecDirectory
        {
            get
            {
                string location = Assembly.GetExecutingAssembly().Location;
                return Path.GetFileName(location);
            }
        }



    }
}
