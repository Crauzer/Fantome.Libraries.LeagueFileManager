﻿using System;
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
                FileInfo fileToInstall = new FileInfo(filePath);
                if (!fileToInstall.Exists)
                {
                    throw new FileToInstallNotFoundException();
                }
                bool newFile = false;
                ReleaseManifest.ReleaseManifestFileEntry fileEntry = this.GameManifest.GetFile(gamePath, false);
                if (fileEntry == null)
                {
                    fileEntry = this.GameManifest.GetFile(gamePath, true);
                    newFile = true;
                }
                fileEntry.DeployMode = deployMode;
                fileEntry.SizeRaw = (int)fileToInstall.Length;
                fileEntry.SizeRaw = fileEntry.SizeCompressed;
                fileEntry.Version = FantomeFilesVersion;
                try
                {
                    string installPath = this.GetFileToInstallPath(fileEntry);
                    Directory.CreateDirectory(Path.GetDirectoryName(installPath));
                    File.Copy(filePath, installPath, true);
                    this.GameManifest.Save();
                }
                catch (Exception e)
                {
                    // In case installing a new file fails, remove it from the manifest (since it's not installed).
                    if (newFile)
                    {
                        fileEntry.Remove();
                    }
                    throw e;
                }

            }

            private string GetFileToInstallPath(ReleaseManifest.ReleaseManifestFileEntry fileEntry)
            {
                if (fileEntry.DeployMode == ReleaseManifest.DeployMode.Managed)
                {
                    return String.Format("{0}/managedfiles/{1}/{2}", this.Project.GetFolder(), LeagueInstallation.GetReleaseString(fileEntry.Version), fileEntry.GetFullPath());
                }
                else if (fileEntry.DeployMode == ReleaseManifest.DeployMode.Deployed)
                {
                    return String.Format("{0}/releases/{1}/deploy/{2}", this.Project.GetFolder(), this.Version, fileEntry.GetFullPath());
                }
                else
                {
                    throw new UnsupportedDeployModeException();
                }
            }

            public class ReleaseManifestNotFoundException : Exception
            {
                public ReleaseManifestNotFoundException() : base("The release manifest was not found for this release.") { }
            }

            public class FileToInstallNotFoundException : Exception
            {
                public FileToInstallNotFoundException() : base("The specified file to install doesn't exist.") { }
            }

            public class UnsupportedDeployModeException : Exception
            {
                public UnsupportedDeployModeException() : base("The specified deploy mode is not supported yet.") { }
            }
        }
    }

}
