using System;

namespace Fantome.Libraries.LeagueFileManager
{
    internal class LeagueRawInstallation : LeagueInstallation
    {
        public LeagueRawInstallation(LeagueManager currentManager, string folder) : base(currentManager, folder)
        {

        }

        public override void InstallFile(ModifiedFile modifiedFile)
        {
            throw new NotImplementedException();
        }

        public override void RevertFile(ModifiedFile modifiedFile)
        {
            throw new NotImplementedException();
        }
    }
}