using System.IO;
using Fantome.Libraries.LeagueFileManager.RiotArchive;
using System.Collections.Generic;

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
            using (LeagueManager manager = new LeagueManager("", "C:/Riot Games/League of Legends"))
            {

            }
        }
    }
}
