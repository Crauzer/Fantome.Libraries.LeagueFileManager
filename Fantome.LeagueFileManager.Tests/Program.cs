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
            RAFTests();
            //LeagueInstallationTests();
        }

        private static void RAFTests()
        {
            RAF raf = new RAF(@"D:\Jeux\LeagueOfLegends\RADS\projects\lol_game_client\filearchives\0.0.0.25\Archive_1.raf");
        }

        private static void LeagueInstallationTests()
        {
            LeagueInstallation install = new LeagueInstallation(@"C:\Riot Games\League of Legends");
            var lolilol = install.GetProject("lol_game_client").GetLatestRelease();
        }
    }
}
