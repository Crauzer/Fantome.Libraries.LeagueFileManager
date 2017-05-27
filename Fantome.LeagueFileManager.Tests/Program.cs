using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.LeagueFileManager.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            //RAFTests();
            LeagueInstallationTests();
        }

        private static void RAFTests()
        {
            using (RAF raf = new RAF(@"D:\Chewy\Desktop\Archive_2.raf"))
            {

            }
            RAF raf2 = new RAF(@"D:\Chewy\Desktop\Archive_3.raf");
        }

        private static void LeagueInstallationTests()
        {
            LeagueInstallation install = new LeagueInstallation(@"C:\Riot Games\League of Legends");
            install.LoadOriginalManifests();
            var lolilol = install.GetProject("lol_game_client").GetLatestRelease();
            lolilol.RevertFile("LEVELS/map11/scene/textures/grnd_terrain_r.dds");
        }
    }
}
