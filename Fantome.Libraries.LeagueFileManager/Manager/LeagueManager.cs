using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Libraries.LeagueFileManager
{
    public partial class LeagueManager : IDisposable
    {
        private LeagueInstallation Installation;
        public LeagueDeployRules DeployRules { get; private set; }

        public LeagueManager(string gamePath)
        {
            this.Installation = new LeagueInstallation(gamePath);
            this.Installation.LoadOriginalManifests();
            this.DeployRules = new LeagueDeployRules(LeagueFileDeployMode.Managed);
        }

        public void InstallFile(string projectName, string gamePath, string filePath)
        {
            this.GetProjectLatestRelease(projectName).InstallFile(gamePath, filePath, DeployRules);
        }

        public void RevertFile(string projectName, string gamePath)
        {
            this.GetProjectLatestRelease(projectName).RevertFile(gamePath);
        }

        private LeagueProjectRelease GetProjectLatestRelease(string projectName)
        {
            LeagueProject foundProject = Installation.GetProject(projectName);
            if (foundProject == null)
            {
                throw new ProjectNotFoundException();
            }
            LeagueProjectRelease foundProjectRelease = foundProject.GetLatestRelease();
            if (foundProjectRelease == null)
            {
                throw new ProjectReleaseNotFoundException();
            }
            return foundProjectRelease;
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

        public class ProjectNotFoundException : Exception
        {
            public ProjectNotFoundException() : base("The specified project was not found in the current League Installation") { }
        }

        public class ProjectReleaseNotFoundException : Exception
        {
            public ProjectReleaseNotFoundException() : base("The release was not found for the specified project") { }
        }
    }
}
