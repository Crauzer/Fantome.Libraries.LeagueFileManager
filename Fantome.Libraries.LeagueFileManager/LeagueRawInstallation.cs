using System;

namespace Fantome.Libraries.LeagueFileManager
{
    internal class LeagueRawInstallation : LeagueInstallation
    {
        public LeagueRawInstallation(string managerInstallationFolder, string folder) : base(managerInstallationFolder, folder)
        {

        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void InstallFile(string gamePath, string filePath, byte[] md5)
        {
            throw new NotImplementedException();
        }

        public override void RevertFile(string gamePath, byte[] md5)
        {
            throw new NotImplementedException();
        }
    }
}