using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.LeagueFileManager
{
    public partial class LeagueManager : IDisposable
    {
        private LeagueInstallation Installation;

        public LeagueManager(string gamePath)
        {
            this.Installation = new LeagueInstallation(gamePath);
            this.Installation.LoadOriginalManifests();
        }



        public void Dispose()
        {
            foreach (LeagueProject project in this.Installation.Projects)
            {
                foreach (LeagueProjectRelease release in project.Releases)
                {
                    if(release.HasChanged)
                    {
                        release.GameManifest.Save();
                    }
                }
            }
        }
    }
}
