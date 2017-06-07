using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.LeagueFileManager
{
    public partial class LeagueManager : IDisposable
    {
        private LeagueInstallation Installation;
        public DeployModeRules DeployRules { get; private set; }

        public LeagueManager(string gamePath)
        {
            this.Installation = new LeagueInstallation(gamePath);
            this.Installation.LoadOriginalManifests();
            this.DeployRules = new DeployModeRules(DeployModeRules.FileDeployMode.SolutionDeployed);
        }

        public void Dispose()
        {
            foreach (LeagueProject project in this.Installation.Projects)
            {
                foreach (LeagueProjectRelease release in project.Releases)
                {
                    if (release.HasChanged)
                    {
                        release.GameManifest.Save();
                    }
                }
            }
        }

        public class DeployModeRules
        {
            public List<DeployModeRule> Rules { get; private set; } = new List<DeployModeRule>();

            public DeployModeRules(FileDeployMode defaultTargetDeployMode)
            {
                this.Rules.Add(new DeployModeRule(null, defaultTargetDeployMode));
            }

            public FileDeployMode GetTargetDeployMode(string project, ReleaseManifest.ReleaseManifestFileEntry originalFileEntry)
            {
                DeployModeRule foundRule = this.Rules.Find(x => x.Project == project);
                if (foundRule == null)
                {
                    foundRule = this.Rules.Find(x => x.Project == null);
                }
                FileDeployMode originalDeployMode = FileDeployMode.Default;

                if (originalFileEntry != null)
                {
                    switch (originalFileEntry.DeployMode)
                    {
                        case ReleaseManifest.DeployMode.Deployed:
                            originalDeployMode = FileDeployMode.Deployed;
                            break;
                        case ReleaseManifest.DeployMode.Managed:
                            originalDeployMode = FileDeployMode.Managed;
                            break;
                        case ReleaseManifest.DeployMode.RAFCompressed:
                            originalDeployMode = FileDeployMode.RAFCompressed;
                            break;
                        case ReleaseManifest.DeployMode.RAFRaw:
                            originalDeployMode = FileDeployMode.RAFRaw;
                            break;
                        case ReleaseManifest.DeployMode.SolutionDeployed:
                            originalDeployMode = FileDeployMode.SolutionDeployed;
                            break;
                    }
                }
                return foundRule.GetTargetDeployMode(originalDeployMode);
            }

            public class DeployModeRule
            {
                public string Project { get; private set; }
                public List<DeployModeProjectRule> ProjectRules { get; private set; } = new List<DeployModeProjectRule>();

                public DeployModeRule(string projectName, FileDeployMode defaultTargetDeployMode)
                {
                    this.Project = projectName;
                    this.ProjectRules.Add(new DeployModeProjectRule(FileDeployMode.Default, defaultTargetDeployMode));
                }

                public FileDeployMode GetTargetDeployMode(FileDeployMode originalDeployMode)
                {
                    DeployModeProjectRule foundRule = this.ProjectRules.Find(x => x.OriginalFileDeployMode == originalDeployMode);
                    if (foundRule != null)
                    {
                        return foundRule.TargetDeployMode;
                    }
                    else
                    {
                        return this.ProjectRules.Find(x => x.OriginalFileDeployMode == FileDeployMode.Default).TargetDeployMode;
                    }
                }
            }

            public class DeployModeProjectRule
            {
                public FileDeployMode OriginalFileDeployMode { get; private set; }
                public FileDeployMode TargetDeployMode { get; private set; }

                public DeployModeProjectRule(FileDeployMode originalFileDeployMode, FileDeployMode targetDeployMode)
                {
                    this.OriginalFileDeployMode = originalFileDeployMode;
                    this.TargetDeployMode = targetDeployMode;
                }
            }

            public enum FileDeployMode
            {
                Default,
                RAFRaw,
                RAFCompressed,
                Managed,
                Deployed,
                SolutionDeployed
            }
        }


    }
}
