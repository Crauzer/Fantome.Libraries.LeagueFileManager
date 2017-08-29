using System;

namespace Fantome.Libraries.LeagueFileManager.Installation
{
    internal class LeagueRawInstallation : LeagueInstallation
    {
        public LeagueRawInstallation(string managerInstallationFolder, string folder) : base(managerInstallationFolder, folder)
        {

        }

        public override void Dispose()
        {
       
        }

        public override void InstallFile(string gamePath, string filePath, byte[] md5)
        {
          
        }

        public override void RevertFile(string gamePath, byte[] md5)
        {
            
        }
    }
}