using System;
using System.Collections.Generic;
using static Fantome.Libraries.LeagueFileManager.ReleaseManifest.ReleaseManifestFile;

namespace Fantome.Libraries.LeagueFileManager
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
                    case DeployMode.Deployed0:
                        originalDeployMode = LeagueFileDeployMode.Deployed0;
                        break;
                    case DeployMode.Managed:
                        originalDeployMode = LeagueFileDeployMode.Managed;
                        break;
                    case DeployMode.RAFCompressed:
                        originalDeployMode = LeagueFileDeployMode.RAFCompressed;
                        break;
                    case DeployMode.RAFRaw:
                        originalDeployMode = LeagueFileDeployMode.RAFRaw;
                        break;
                    case DeployMode.Deployed4:
                        originalDeployMode = LeagueFileDeployMode.Deployed4;
                        break;
                }
            }
            return foundRule.GetTargetDeployMode(originalDeployMode);
        }

        public DeployMode GetTargetDeployMode(string project, ReleaseManifest.ReleaseManifestFileEntry originalFileEntry)
        {
            LeagueFileDeployMode targetDeployMode = GetTargetLeagueDeployMode(project, originalFileEntry);
            switch (targetDeployMode)
            {
                case LeagueFileDeployMode.Deployed0:
                    return DeployMode.Deployed0;
                case LeagueFileDeployMode.Managed:
                    return DeployMode.Managed;
                case LeagueFileDeployMode.RAFCompressed:
                    return DeployMode.RAFCompressed;
                case LeagueFileDeployMode.RAFRaw:
                    return DeployMode.RAFRaw;
                case LeagueFileDeployMode.Deployed4:
                    return DeployMode.Deployed4;
                default:
                    throw new InvalidTargetDeployModeException();
            }
        }

        protected class LeagueDeployModeRule
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

            protected class LeagueDeployModeProjectRule
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
        }

        public enum LeagueFileDeployMode
        {
            Default,
            RAFRaw,
            RAFCompressed,
            Managed,
            Deployed0,
            Deployed4
        }

        public class InvalidTargetDeployModeException : Exception
        {
            public InvalidTargetDeployModeException() : base("The target deploy mode cannot be Default") { }
        }
    }
}