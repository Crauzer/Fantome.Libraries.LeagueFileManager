using System;

namespace Fantome.Libraries.LeagueFileManager
{
    public partial class LeagueManager
    {
        protected class LeagueRawInstallation : LeagueInstallation
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
}
