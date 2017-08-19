namespace Fantome.Libraries.LeagueFileManager
{
    public class ModifiedFile
    {
        public string GamePath { get; private set; }

        public string Project { get; set; }        

        public string FilePath { get; set; }

        public byte[] MD5 { get; set; }        

        public ModifiedFile(string gamePath)
        {
            this.GamePath = gamePath;
        }
    }
}