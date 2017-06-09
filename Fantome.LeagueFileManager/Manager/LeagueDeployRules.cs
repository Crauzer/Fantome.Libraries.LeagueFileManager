using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.LeagueFileManager
{
    public partial class LeagueManager
    {
        public class LeagueDeployRules
        {
            private List<LeagueDeployModeRule> _rules = new List<LeagueDeployModeRule>();

            public LeagueDeployRules(LeagueFileDeployMode defaultTargetDeployMode)
            {
                this.AddDeployModeRule(null, LeagueFileDeployMode.Default, defaultTargetDeployMode);
            }

            public void AddDeployModeRule(string projectName, LeagueFileDeployMode originalDeployMode, LeagueFileDeployMode targetDeployMode)
            {
                LeagueDeployModeRule foundDeployModeRule = _rules.Find(x => x.Project == projectName);
                if (foundDeployModeRule == null)
                {
                    foundDeployModeRule = new LeagueDeployModeRule(projectName);
                    _rules.Add(foundDeployModeRule);
                }
                foundDeployModeRule.AddDeployModeProjectRule(originalDeployMode, targetDeployMode);
            }

            private LeagueFileDeployMode GetTargetLeagueDeployMode(string project, ReleaseManifest.ReleaseManifestFileEntry originalFileEntry)
            {
                LeagueDeployModeRule foundRule = _rules.Find(x => x.Project == project);
                if (foundRule == null)
                {
                    foundRule = _rules.Find(x => x.Project == null);
                }
                LeagueFileDeployMode originalDeployMode = LeagueFileDeployMode.Default;

                if (originalFileEntry != null)
                {
                    switch (originalFileEntry.DeployMode)
                    {
                        case ReleaseManifest.DeployMode.Deployed:
                            originalDeployMode = LeagueFileDeployMode.Deployed;
                            break;
                        case ReleaseManifest.DeployMode.Managed:
                            originalDeployMode = LeagueFileDeployMode.Managed;
                            break;
                        case ReleaseManifest.DeployMode.RAFCompressed:
                            originalDeployMode = LeagueFileDeployMode.RAFCompressed;
                            break;
                        case ReleaseManifest.DeployMode.RAFRaw:
                            originalDeployMode = LeagueFileDeployMode.RAFRaw;
                            break;
                        case ReleaseManifest.DeployMode.SolutionDeployed:
                            originalDeployMode = LeagueFileDeployMode.SolutionDeployed;
                            break;
                    }
                }
                return foundRule.GetTargetDeployMode(originalDeployMode);
            }

            public ReleaseManifest.DeployMode GetTargetDeployMode(string project, ReleaseManifest.ReleaseManifestFileEntry originalFileEntry)
            {
                LeagueFileDeployMode targetDeployMode = GetTargetLeagueDeployMode(project, originalFileEntry);
                switch (targetDeployMode)
                {
                    case LeagueFileDeployMode.Deployed:
                        return ReleaseManifest.DeployMode.Deployed;
                    case LeagueFileDeployMode.Managed:
                        return ReleaseManifest.DeployMode.Managed;
                    case LeagueFileDeployMode.RAFCompressed:
                        return ReleaseManifest.DeployMode.RAFCompressed;
                    case LeagueFileDeployMode.RAFRaw:
                        return ReleaseManifest.DeployMode.RAFRaw;
                    case LeagueFileDeployMode.SolutionDeployed:
                        return ReleaseManifest.DeployMode.SolutionDeployed;
                    default:
                        throw new InvalidTargetDeployModeException();
                }
            }
        }

        public class LeagueDeployModeRule
        {
            public string Project { get; private set; }
            private List<LeagueDeployModeProjectRule> _projectRules = new List<LeagueDeployModeProjectRule>();

            public LeagueDeployModeRule(string projectName)
            {
                this.Project = projectName;
            }

            public void AddDeployModeProjectRule(LeagueFileDeployMode originalDeployMode, LeagueFileDeployMode targetDeployMode)
            {
                LeagueDeployModeProjectRule foundDeployModeProjectRule = _projectRules.Find(x => x.OriginalFileDeployMode == originalDeployMode);
                if (foundDeployModeProjectRule != null)
                {
                    _projectRules.Remove(foundDeployModeProjectRule);
                }
                _projectRules.Add(new LeagueDeployModeProjectRule(originalDeployMode, targetDeployMode));
            }

            public LeagueFileDeployMode GetTargetDeployMode(LeagueFileDeployMode originalDeployMode)
            {
                LeagueDeployModeProjectRule foundRule = _projectRules.Find(x => x.OriginalFileDeployMode == originalDeployMode);
                if (foundRule != null)
                {
                    return foundRule.TargetDeployMode;
                }
                else
                {
                    return _projectRules.Find(x => x.OriginalFileDeployMode == LeagueFileDeployMode.Default).TargetDeployMode;
                }
            }
        }

        public class LeagueDeployModeProjectRule
        {
            public LeagueFileDeployMode OriginalFileDeployMode { get; private set; }
            public LeagueFileDeployMode TargetDeployMode { get; private set; }

            public LeagueDeployModeProjectRule(LeagueFileDeployMode originalFileDeployMode, LeagueFileDeployMode targetDeployMode)
            {
                this.OriginalFileDeployMode = originalFileDeployMode;
                if (targetDeployMode == LeagueFileDeployMode.Default)
                {
                    throw new InvalidTargetDeployModeException();
                }
                this.TargetDeployMode = targetDeployMode;
            }
        }

        public enum LeagueFileDeployMode
        {
            Default,
            RAFRaw,
            RAFCompressed,
            Managed,
            Deployed,
            SolutionDeployed
        }

        public class InvalidTargetDeployModeException : Exception
        {
            public InvalidTargetDeployModeException() : base("The target deploy mode cannot be Default") { }
        }
    }
}
