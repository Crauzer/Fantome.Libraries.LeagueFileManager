namespace Fantome.Libraries.LeagueFileManager
{
    internal abstract class LeagueInstallation
    {
        public readonly string ManagerInstallationFolder;

        public string Folder { get; private set; }

        public LeagueInstallation(string managerInstallationFolder, string folder)
        {
            ManagerInstallationFolder = managerInstallationFolder;
            Folder = folder;
        }

        public abstract void InstallFile(ModifiedFile modifiedFile);

        public abstract void RevertFile(ModifiedFile modifiedFile);
    }
}