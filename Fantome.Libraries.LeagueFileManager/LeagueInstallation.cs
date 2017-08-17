namespace Fantome.Libraries.LeagueFileManager
{
    public partial class LeagueManager
    {
        protected abstract class LeagueInstallation
        {
            public readonly LeagueManager CurrentManager;

            public string Folder { get; private set; }

            public LeagueInstallation(LeagueManager currentManager, string folder)
            {
                CurrentManager = currentManager;
                this.Folder = folder;
            }

            public abstract void InstallFile(ModifiedFile modifiedFile);

            public abstract void RevertFile(ModifiedFile modifiedFile);
        }
    }
}