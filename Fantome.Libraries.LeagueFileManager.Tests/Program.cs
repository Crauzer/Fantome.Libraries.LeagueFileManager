using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Fantome.Libraries.LeagueFileManager.Tests
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
            using (LeagueManager manager = new LeagueManager("C:/Riot Games/League of Legends"))
            {
                manager.DeployRules.AddDeployModeRule("lol_game_client", LeagueManager.LeagueFileDeployMode.Default, LeagueManager.LeagueFileDeployMode.Managed);
                foreach (string file in Directory.EnumerateFiles(@"D:\Chewy\Desktop\Mystic Rift", "*.*", SearchOption.AllDirectories))
                {
                    string gamePath = file.Replace(@"D:\Chewy\Desktop\Mystic Rift\lol_game_client\", "").Replace("\\", "/");
                    //manager.InstallFile("lol_game_client", gamePath, file);
                    manager.RevertFile("lol_game_client", gamePath);
                }
            }
        }
    }
}
