using System;
using System.Collections.Generic;

namespace Fantome.LeagueFileManager
{
    public partial class LeagueInstallation
    {
        public class LeagueProject
        {
            public LeagueInstallation Installation { get; private set; }
            public string Name { get; private set; }
            public ReleaseManifest.DeployMode DefaultDeployMode { get; private set; }
            public List<LeagueProjectRelease> Releases { get; private set; } = new List<LeagueProjectRelease>();

            public LeagueProject(LeagueInstallation installation, string projectName, List<string> releases)
            {
                this.Installation = installation;
                this.Name = projectName;
                this.DefaultDeployMode = this.GetDefaultDeployMode();
                foreach (string release in releases)
                {
                    try
                    {
                        this.Releases.Add(new LeagueProjectRelease(this, release));
                    }
                    catch (LeagueProjectRelease.ReleaseManifestNotFoundException)
                    {
                    }
                }
                if (this.Releases.Count == 0)
                {
                    throw new NoValidReleaseException();
                }
            }

            public void LoadOriginalManifests()
            {
                foreach(LeagueProjectRelease release in this.Releases)
                {
                    release.LoadOriginalManifest();
                }
            }

            private ReleaseManifest.DeployMode GetDefaultDeployMode()
            {
                if (this.Name.Equals("lol_game_client"))
                {
                    return ReleaseManifest.DeployMode.RAFRaw;
                }
                else if (this.Name.StartsWith("lol_game_client_"))
                {
                    return ReleaseManifest.DeployMode.Managed;
                }
                else
                {
                    return ReleaseManifest.DeployMode.Deployed;
                }
            }

            public string GetFolder()
            {
                return String.Format("{0}/RADS/projects/{1}", this.Installation.Folder, this.Name);
            }

            public LeagueProjectRelease GetRelease(string version)
            {
                return this.Releases.Find(x => x.Version == version);
            }

            public LeagueProjectRelease GetLatestRelease()
            {
                LeagueProjectRelease latestRelease = null;
                foreach (LeagueProjectRelease release in this.Releases)
                {
                    uint releaseValue = GetReleaseValue(release.Version);
                    if (latestRelease == null || releaseValue > latestRelease.VersionValue)
                    {
                        latestRelease = release;
                    }
                }
                return latestRelease;
            }

            public class NoValidReleaseException : Exception
            {
                public NoValidReleaseException() : base("There is no valid release for this project.") { }
            }
        }
    }

}
