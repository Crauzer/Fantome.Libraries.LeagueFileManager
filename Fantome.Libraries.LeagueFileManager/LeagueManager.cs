using System;
using System.IO;

namespace Fantome.Libraries.LeagueFileManager
{
    public partial class LeagueManager : IDisposable
    {
        private readonly LeagueInstallation _installation;
        private readonly string _managerFolderPath;

        public LeagueManager(string managerFolderPath, string gamePath)
        {
            _managerFolderPath = managerFolderPath;
            if (Directory.Exists(gamePath + "/RADS"))
            {
                _installation = new LeagueRADSInstallation(this, gamePath);
            }
            else
            {
                _installation = new LeagueRawInstallation(this, gamePath);
            }
        }

        public void InstallFile(ModifiedFile modifiedFile)
        {
            _installation.InstallFile(modifiedFile);
        }

        public void RevertFile(ModifiedFile modifiedFile)
        {
            _installation.RevertFile(modifiedFile);
        }

        public void AddDeployModeRule(string projectName, LeagueRADSFileDeployMode originalDeployMode, LeagueRADSFileDeployMode targetDeployMode)
        {
            (_installation as LeagueRADSInstallation)?.DeployRules.AddDeployModeRule(projectName, originalDeployMode, targetDeployMode);
        }

        public void Dispose()
        {
            if (_installation is LeagueRADSInstallation)
            {
                foreach (LeagueRADSProject project in (_installation as LeagueRADSInstallation).Projects)
                {
                    foreach (LeagueRADSProjectRelease release in project.Releases)
                    {
                        if (release.HasChanged)
                        {
                            release.GameManifest.Save();
                        }
                    }
                }
            }
        }
    }
}
