using System;
using System.Collections.Generic;

namespace Fantome.Libraries.LeagueFileManager
{
    internal class LeagueRADSProject
    {
        public readonly LeagueRADSInstallation Installation;
        public readonly string Name;
        public readonly List<LeagueRADSProjectRelease> Releases = new List<LeagueRADSProjectRelease>();

        public LeagueRADSProject(LeagueRADSInstallation installation, string projectName, List<string> releases)
        {
            this.Installation = installation;
            this.Name = projectName;
            foreach (string release in releases)
            {
                try
                {
                    this.Releases.Add(new LeagueRADSProjectRelease(this, release));
                }
                catch (LeagueRADSProjectRelease.ReleaseManifestNotFoundException)
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
            foreach (LeagueRADSProjectRelease release in this.Releases)
            {
                release.LoadOriginalManifest();
            }
        }

        public string GetFolder()
        {
            return String.Format("{0}/RADS/projects/{1}", this.Installation.Folder, this.Name);
        }

        public LeagueRADSProjectRelease GetRelease(string version)
        {
            return this.Releases.Find(x => x.Version == version);
        }

        public LeagueRADSProjectRelease GetLatestRelease()
        {
            LeagueRADSProjectRelease latestRelease = null;
            foreach (LeagueRADSProjectRelease release in this.Releases)
            {
                uint releaseValue = LeagueRADSInstallation.GetReleaseValue(release.Version);
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
