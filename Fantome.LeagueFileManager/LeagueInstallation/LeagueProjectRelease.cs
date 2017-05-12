using System;
using System.IO;

namespace Fantome.LeagueFileManager
{
    public partial class LeagueInstallation
    {
        public class LeagueProjectRelease
        {
            public LeagueProject Project { get; private set; }
            public string Version { get; private set; }
            public uint VersionValue { get; private set; }
            public ReleaseManifest GameManifest { get; private set; }
            public ReleaseManifest OriginalManifest { get; private set; }

            public LeagueProjectRelease(LeagueProject project, string version)
            {
                this.Project = project;
                this.Version = version;
                this.VersionValue = GetReleaseValue(version);
                string manifestPath = this.GetFolder() + "/releasemanifest";
                if (File.Exists(manifestPath))
                {
                    this.GameManifest = new ReleaseManifest(manifestPath);
                    this.OriginalManifest = this.GetOriginalManifest(manifestPath);
                }
                else
                {
                    throw new ReleaseManifestNotFoundException();
                }
            }

            private ReleaseManifest GetOriginalManifest(string gameManifestPath)
            {
                string originalManifestFolder = String.Format("{0}manifests/{1}/{2}", AppDomain.CurrentDomain.BaseDirectory, this.Project.Name, this.Version);
                string manifestPath = originalManifestFolder + "/releasemanifest";
                Directory.CreateDirectory(originalManifestFolder);
                if (!File.Exists(manifestPath))
                {
                    File.Copy(gameManifestPath, manifestPath);
                }
                return new ReleaseManifest(manifestPath);
            }

            public string GetFolder()
            {
                return String.Format("{0}/releases/{1}", this.Project.GetFolder(), this.Version);
            }

            public void InstallFile(string gamePath, ReleaseManifest.DeployMode deployMode, string filePath)
            {

            }

            public class ReleaseManifestNotFoundException : Exception
            {
                public ReleaseManifestNotFoundException() : base("The release manifest was not found for this release.") { }
            }
        }
    }

}
