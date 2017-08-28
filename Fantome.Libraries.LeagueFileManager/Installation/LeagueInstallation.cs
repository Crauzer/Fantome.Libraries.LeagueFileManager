using System;

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

        public abstract void InstallFile(string gamePath, string filePath, byte[] md5);

        public abstract void RevertFile(string gamePath, byte[] md5);

        public abstract void Dispose();
    }
}