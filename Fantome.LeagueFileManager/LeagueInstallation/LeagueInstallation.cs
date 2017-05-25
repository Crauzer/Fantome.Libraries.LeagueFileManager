using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Fantome.LeagueFileManager
{
    public partial class LeagueInstallation
    {
        public string Folder { get; private set; }
        public List<LeagueProject> Projects { get; private set; } = new List<LeagueProject>();
        public static uint FantomeFilesVersion { get; private set; } = GetReleaseValue("11.6.17.2");

        public LeagueInstallation(string gameFolder)
        {
            this.Folder = gameFolder;
            if (!Directory.Exists(gameFolder + "/RADS"))
            {
                throw new InvalidRADSInstallationException("RADS folder doesn't exist.");
            }
            else if (!Directory.Exists(gameFolder + "/RADS/projects"))
            {
                throw new InvalidRADSInstallationException("projects folder doesn't exist.");
            }
            this.LoadProjects();
        }

        public void LoadProjects()
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
                            LeagueProject newProject = new LeagueProject(this, dirInfo.Name, releases);
                            this.Projects.Add(newProject);
                        }
                        catch (LeagueProject.NoValidReleaseException)
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

        public void LoadOriginalManifests()
        {
            foreach (LeagueProject project in this.Projects)
            {
                project.LoadOriginalManifests();
            }
        }

        public LeagueProject GetProject(string projectName)
        {
            return this.Projects.Find(x => String.Equals(x.Name, projectName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool IsReleaseVersion(string releaseString)
        {
            // Sometimes the folder in releases can be called "installer"
            return (releaseString.Split('.').Length == 4);
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
    }
}
