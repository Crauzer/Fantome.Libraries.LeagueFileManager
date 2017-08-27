using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Fantome.Libraries.LeagueFileManager
{
    public class LeagueManager : IDisposable
    {
        private readonly LeagueInstallation _installation;
        private readonly string _managerFolderPath;

        public LeagueManager(string managerFolderPath, string gamePath)
        {
            _managerFolderPath = managerFolderPath;
            string managerInstallationFolder = GetManagerInstallationFolder(gamePath);
            if (Directory.Exists(gamePath + "/RADS"))
            {
                _installation = new LeagueRADSInstallation(managerInstallationFolder, gamePath);
            }
            else
            {
                _installation = new LeagueRawInstallation(managerInstallationFolder, gamePath);
            }
        }

        public void InstallFile(string gamePath, string filePath, byte[] md5)
        {
            _installation.InstallFile(gamePath, filePath, md5);
        }

        public void InstallFile(string gamePath, string filePath)
        {
            InstallFile(gamePath, filePath, null);
        }

        public void RevertFile(string gamePath, byte[] md5)
        {
            _installation.RevertFile(gamePath, md5);
        }

        public void RevertFile(string gamePath)
        {
            RevertFile(gamePath, null);
        }

        public void AddDeployModeRule(LeagueRADSFileDeployMode originalDeployMode, LeagueRADSFileDeployMode targetDeployMode)
        {
            AddDeployModeRule(originalDeployMode, targetDeployMode, null);
        }

        public void AddDeployModeRule(LeagueRADSFileDeployMode originalDeployMode, LeagueRADSFileDeployMode targetDeployMode, string projectName)
        {
            (_installation as LeagueRADSInstallation)?.DeployRules.AddDeployModeRule(projectName, originalDeployMode, targetDeployMode);
        }

        public void Dispose()
        {
            _installation.Dispose();
        }

        private string GetManagerInstallationFolder(string gamePath)
        {
            List<InstallationInfo> savedInstallations = GetSavedInstallations();
            InstallationInfo foundInstallation = savedInstallations.Find(x => x.Folder.Replace("\\", "/").Equals(gamePath.Replace("\\", "/"), StringComparison.InvariantCultureIgnoreCase));
            if (foundInstallation == null)
            {
                int currentID = 1;
                while (savedInstallations.Find(x => x.ID == currentID) != null)
                {
                    currentID++;
                }
                foundInstallation = new InstallationInfo(gamePath, currentID);
                savedInstallations.Add(foundInstallation);
                SaveInstallations(savedInstallations);
            }
            return Path.Combine(_managerFolderPath, "lol-manager", foundInstallation.ID.ToString());
        }

        private List<InstallationInfo> GetSavedInstallations()
        {
            string installationsListPath = Path.Combine(_managerFolderPath, "lol-manager", "installations.json");
            if (File.Exists(installationsListPath))
            {
                return JsonConvert.DeserializeObject<List<InstallationInfo>>(File.ReadAllText(installationsListPath));
            }
            else
            {
                return new List<InstallationInfo>();
            }
        }

        private void SaveInstallations(List<InstallationInfo> installations)
        {
            string installationsListPath = Path.Combine(_managerFolderPath, "lol-manager", "installations.json");
            Directory.CreateDirectory(Path.GetDirectoryName(installationsListPath));
            File.WriteAllText(installationsListPath, JsonConvert.SerializeObject(installations));
        }

        private class InstallationInfo
        {
            public readonly string Folder;
            public readonly int ID;

            public InstallationInfo(string folder, int id)
            {
                Folder = folder;
                ID = id;
            }
        }
    }
}
