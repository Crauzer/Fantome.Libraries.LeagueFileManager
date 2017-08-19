using System;
using System.Collections.Generic;
using static Fantome.Libraries.LeagueFileManager.ReleaseManifest.ReleaseManifestFile;

namespace Fantome.Libraries.LeagueFileManager
{
    internal class LeagueRADSDeployRules
    {
        private readonly List<LeagueRADSDeployModeRule> _rules = new List<LeagueRADSDeployModeRule>();

        public LeagueRADSDeployRules(LeagueRADSFileDeployMode defaultTargetDeployMode)
        {
            this.AddDeployModeRule(null, LeagueRADSFileDeployMode.Default, defaultTargetDeployMode);
        }

        public void AddDeployModeRule(string projectName, LeagueRADSFileDeployMode originalDeployMode, LeagueRADSFileDeployMode targetDeployMode)
        {
            LeagueRADSDeployModeRule foundDeployModeRule = _rules.Find(x => x.Project == projectName);
            if (foundDeployModeRule == null)
            {
                foundDeployModeRule = new LeagueRADSDeployModeRule(projectName);
                _rules.Add(foundDeployModeRule);
            }
            foundDeployModeRule.AddDeployModeProjectRule(originalDeployMode, targetDeployMode);
        }

        private LeagueRADSFileDeployMode GetTargetLeagueDeployMode(string project, ReleaseManifest.ReleaseManifestFileEntry originalFileEntry)
        {
            LeagueRADSDeployModeRule foundRule = _rules.Find(x => x.Project == project);
            if (foundRule == null)
            {
                foundRule = _rules.Find(x => x.Project == null);
            }
            LeagueRADSFileDeployMode originalDeployMode = LeagueRADSFileDeployMode.Default;

            if (originalFileEntry != null)
            {
                switch (originalFileEntry.DeployMode)
                {
                    case DeployMode.Deployed0:
                        originalDeployMode = LeagueRADSFileDeployMode.Deployed0;
                        break;
                    case DeployMode.Managed:
                        originalDeployMode = LeagueRADSFileDeployMode.Managed;
                        break;
                    case DeployMode.RAFCompressed:
                        originalDeployMode = LeagueRADSFileDeployMode.RAFCompressed;
                        break;
                    case DeployMode.RAFRaw:
                        originalDeployMode = LeagueRADSFileDeployMode.RAFRaw;
                        break;
                    case DeployMode.Deployed4:
                        originalDeployMode = LeagueRADSFileDeployMode.Deployed4;
                        break;
                }
            }
            return foundRule.GetTargetDeployMode(originalDeployMode);
        }

        public DeployMode GetTargetDeployMode(string project, ReleaseManifest.ReleaseManifestFileEntry originalFileEntry)
        {
            LeagueRADSFileDeployMode targetDeployMode = GetTargetLeagueDeployMode(project, originalFileEntry);
            switch (targetDeployMode)
            {
                case LeagueRADSFileDeployMode.Deployed0:
                    return DeployMode.Deployed0;
                case LeagueRADSFileDeployMode.Managed:
                    return DeployMode.Managed;
                case LeagueRADSFileDeployMode.RAFCompressed:
                    return DeployMode.RAFCompressed;
                case LeagueRADSFileDeployMode.RAFRaw:
                    return DeployMode.RAFRaw;
                case LeagueRADSFileDeployMode.Deployed4:
                    return DeployMode.Deployed4;
                default:
                    throw new InvalidTargetDeployModeException();
            }
        }

        private class LeagueRADSDeployModeRule
        {
            public readonly string Project;
            private readonly List<LeagueRADSDeployModeProjectRule> _projectRules = new List<LeagueRADSDeployModeProjectRule>();

            public LeagueRADSDeployModeRule(string projectName)
            {
                this.Project = projectName;
            }

            public void AddDeployModeProjectRule(LeagueRADSFileDeployMode originalDeployMode, LeagueRADSFileDeployMode targetDeployMode)
            {
                LeagueRADSDeployModeProjectRule foundDeployModeProjectRule = _projectRules.Find(x => x.OriginalFileDeployMode == originalDeployMode);
                if (foundDeployModeProjectRule != null)
                {
                    _projectRules.Remove(foundDeployModeProjectRule);
                }
                _projectRules.Add(new LeagueRADSDeployModeProjectRule(originalDeployMode, targetDeployMode));
            }

            public LeagueRADSFileDeployMode GetTargetDeployMode(LeagueRADSFileDeployMode originalDeployMode)
            {
                LeagueRADSDeployModeProjectRule foundRule = _projectRules.Find(x => x.OriginalFileDeployMode == originalDeployMode);
                if (foundRule != null)
                {
                    return foundRule.TargetDeployMode;
                }
                else
                {
                    return _projectRules.Find(x => x.OriginalFileDeployMode == LeagueRADSFileDeployMode.Default).TargetDeployMode;
                }
            }

            private class LeagueRADSDeployModeProjectRule
            {
                public readonly LeagueRADSFileDeployMode OriginalFileDeployMode;
                public readonly LeagueRADSFileDeployMode TargetDeployMode;

                public LeagueRADSDeployModeProjectRule(LeagueRADSFileDeployMode originalFileDeployMode, LeagueRADSFileDeployMode targetDeployMode)
                {
                    this.OriginalFileDeployMode = originalFileDeployMode;
                    if (targetDeployMode == LeagueRADSFileDeployMode.Default)
                    {
                        throw new InvalidTargetDeployModeException();
                    }
                    this.TargetDeployMode = targetDeployMode;
                }
            }
        }

        public class InvalidTargetDeployModeException : Exception
        {
            public InvalidTargetDeployModeException() : base("The target deploy mode cannot be Default") { }
        }
    }

    public enum LeagueRADSFileDeployMode
    {
        Default,
        RAFRaw,
        RAFCompressed,
        Managed,
        Deployed0,
        Deployed4
    }
}