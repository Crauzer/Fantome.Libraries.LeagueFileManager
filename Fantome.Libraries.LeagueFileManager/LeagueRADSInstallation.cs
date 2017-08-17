using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Fantome.Libraries.LeagueFileManager
{
    public partial class LeagueManager
    {
        protected class LeagueRADSInstallation : LeagueInstallation
        {
            public readonly List<LeagueRADSProject> Projects = new List<LeagueRADSProject>();
            public readonly LeagueRADSDeployRules DeployRules = new LeagueRADSDeployRules(LeagueRADSFileDeployMode.Managed);
            public static readonly uint FantomeFilesVersion = GetReleaseValue("11.6.17.2");

            public LeagueRADSInstallation(LeagueManager currentManager, string gameFolder) : base(currentManager, gameFolder)
            {
                if (!Directory.Exists(gameFolder + "/RADS"))
                {
                    throw new InvalidRADSInstallationException("RADS folder doesn't exist.");
                }
                else if (!Directory.Exists(gameFolder + "/RADS/projects"))
                {
                    throw new InvalidRADSInstallationException("projects folder doesn't exist.");
                }
                this.LoadProjects();
                this.LoadOriginalManifests();
            }

            private void LoadProjects()
            {
                this.Projects.Clear();
                foreach (string project in Directory.EnumerateDirectories(this.Folder + "/RADS/projects"))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(project);
                    string projectFolder = String.Format("{0}/RADS/projects/{1}", this.Folder, dirInfo.Name);
                    if (Directory.Exists(projectFolder + "/releases"))
                    {
                        List<string> releases = Directory.EnumerateDirectories(projectFolder + "/releases").Select(x => { return new DirectoryInfo(x).Name; }).Where(x => IsReleaseVersion(x)).ToList();
                        if (releases.Count() > 0)
                        {
                            try
                            {
                                LeagueRADSProject newProject = new LeagueRADSProject(this, dirInfo.Name, releases);
                                this.Projects.Add(newProject);
                            }
                            catch (LeagueRADSProject.NoValidReleaseException)
                            {
                            }
                        }
                    }
                }
                if (this.Projects.Count == 0)
                {
                    throw new NoValidProjectException();
                }
            }

            private void LoadOriginalManifests()
            {
                foreach (LeagueRADSProject project in this.Projects)
                {
                    project.LoadOriginalManifests();
                }
            }

            public override void InstallFile(ModifiedFile modifiedFile)
            {
                GetProjectLatestRelease(modifiedFile.Project).InstallFile(modifiedFile, DeployRules);
            }

            public override void RevertFile(ModifiedFile modifiedFile)
            {
                GetProjectLatestRelease(modifiedFile.Project).RevertFile(modifiedFile);
            }

            public static uint GetReleaseValue(string releaseString)
            {
                string[] releaseValues = releaseString.Split('.');
                if (releaseValues.Length != 4)
                {
                    throw new InvalidReleaseVersionToParseException();
                }
                return (uint)((Byte.Parse(releaseValues[0]) << 24) | (Byte.Parse(releaseValues[1]) << 16) | (Byte.Parse(releaseValues[2]) << 8) | Byte.Parse(releaseValues[3]));
            }

            public static string GetReleaseString(uint releaseValue)
            {
                return String.Format("{0}.{1}.{2}.{3}", (releaseValue & 0xFF000000) >> 24, (releaseValue & 0x00FF0000) >> 16, (releaseValue & 0x0000FF00) >> 8, releaseValue & 0x000000FF);
            }

            private LeagueRADSProject GetProject(string projectName)
            {
                return this.Projects.Find(x => String.Equals(x.Name, projectName, StringComparison.InvariantCultureIgnoreCase));
            }

            private static bool IsReleaseVersion(string releaseString)
            {
                // Sometimes the folder in releases can be called "installer"
                return (releaseString.Split('.').Length == 4);
            }

            private LeagueRADSProjectRelease GetProjectLatestRelease(string projectName)
            {
                LeagueRADSProject foundProject = GetProject(projectName);
                if (foundProject == null)
                {
                    throw new ProjectNotFoundException();
                }
                LeagueRADSProjectRelease foundProjectRelease = foundProject.GetLatestRelease();
                if (foundProjectRelease == null)
                {
                    throw new ProjectReleaseNotFoundException();
                }
                return foundProjectRelease;
            }

            public class InvalidRADSInstallationException : Exception
            {
                public InvalidRADSInstallationException(string message) : base("The specified folder does not lead to a valid RADS installation. " + message) { }
            }

            public class InvalidReleaseVersionToParseException : Exception
            {
                public InvalidReleaseVersionToParseException() : base("The specified release version string is not valid.") { }
            }

            public class NoValidProjectException : Exception
            {
                public NoValidProjectException() : base("There is no valid project in the specified LoL Installation.") { }
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
}
