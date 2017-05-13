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
            LeagueInstallationTests();

        }

        private static void LeagueInstallationTests()
        {
            LeagueInstallation install = new LeagueInstallation(@"C:\Riot Games\League of Legends");
            var lolilol = install.GetProject("lol_game_client").GetLatestRelease();
            lolilol.InstallFile("DATA/Characters/Zed/Skins/Base/Zed_base_TX_CM.DDS", ReleaseManifest.DeployMode.Managed, @"D:\Files\Documents\LoL\Wooxy debug\extract\maps\lol_game_client\DATA\Characters\Zed\Skins\Base\Zed_base_TX_CM.DDS");
        }
    }
}
