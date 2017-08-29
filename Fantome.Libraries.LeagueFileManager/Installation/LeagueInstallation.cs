using System;
using System.IO;
using System.Security.Cryptography;

namespace Fantome.Libraries.LeagueFileManager.Installation
{
    internal abstract class LeagueInstallation : IDisposable
    {
        public readonly string ManagerInstallationFolder;

        public string Folder { get; private set; }

        public LeagueInstallation(string managerInstallationFolder, string folder)
        {
            ManagerInstallationFolder = managerInstallationFolder;
            Folder = folder;
        }

        public virtual void InstallFile(string gamePath, string filePath, byte[] md5) {
            if (filePath == null)
                throw new NotSpecifiedFileToInstallException();

            if (!File.Exists(filePath))
                throw new FileToInstallNotFoundException();

            if (md5 == null)
            {
                using (var newMD5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        md5 = newMD5.ComputeHash(stream);
                    }
                }
            }
        }

        public abstract void RevertFile(string gamePath, byte[] md5);

        public abstract void Dispose();

        public class FileToInstallNotFoundException : Exception
        {
            public FileToInstallNotFoundException() : base("The specified file to install doesn't exist.") { }
        }

        public class NotSpecifiedFileToInstallException : Exception
        {
            public NotSpecifiedFileToInstallException() : base("The file to install path cannot be null.") { }
        }
    }
}