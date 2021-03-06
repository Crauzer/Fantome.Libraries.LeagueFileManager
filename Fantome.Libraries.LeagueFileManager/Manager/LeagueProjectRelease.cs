﻿using System;
using System.IO;

namespace Fantome.Libraries.LeagueFileManager
{
    public partial class LeagueManager
    {
        protected class LeagueProjectRelease
        {
            public LeagueProject Project { get; private set; }
            public string Version { get; private set; }
            public uint VersionValue { get; private set; }
            public ReleaseManifest GameManifest { get; private set; }
            public ReleaseManifest OriginalManifest { get; private set; }
            public bool HasChanged { get; private set; }

            public LeagueProjectRelease(LeagueProject project, string version)
            {
                this.Project = project;
                this.Version = version;
                this.VersionValue = LeagueInstallation.GetReleaseValue(version);
                string manifestPath = this.GetFolder() + "/releasemanifest";
                if (File.Exists(manifestPath))
                {
                    this.GameManifest = new ReleaseManifest(manifestPath);
                }
                else
                {
                    throw new ReleaseManifestNotFoundException();
                }
            }

            public void LoadOriginalManifest()
            {
                string originalManifestFolder = String.Format("{0}lol-manager/manifests/{1}/{2}",
                    AppDomain.CurrentDomain.BaseDirectory,
                    this.Project.Name,
                    this.Version);
                string manifestPath = originalManifestFolder + "/releasemanifest";
                Directory.CreateDirectory(originalManifestFolder);
                if (!File.Exists(manifestPath))
                {
                    File.Copy(this.GameManifest.FilePath, manifestPath);
                }
                this.OriginalManifest = new ReleaseManifest(manifestPath);
            }

            public string GetFolder()
            {
                return String.Format("{0}/releases/{1}", this.Project.GetFolder(), this.Version);
            }

            public void InstallFile(string gamePath, string filePath, LeagueDeployRules deployRules)
            {
                FileInfo fileToInstall = new FileInfo(filePath);
                if (!fileToInstall.Exists)
                {
                    throw new FileToInstallNotFoundException();
                }

                // Getting the matching file entry (null if new file) and finding the deploy mode to use
                ReleaseManifest.ReleaseManifestFileEntry fileEntry = this.GameManifest.GetFile(gamePath, false);
                ReleaseManifest.DeployMode deployMode = deployRules.GetTargetDeployMode(this.Project.Name, fileEntry);

                // Installing file
                string installPath = this.GetFileToInstallPath(gamePath, deployMode, LeagueInstallation.FantomeFilesVersion);
                Directory.CreateDirectory(Path.GetDirectoryName(installPath));
                if ((fileEntry != null) && deployMode == ReleaseManifest.DeployMode.Deployed)
                {
                    // Backup deployed file
                    BackupFile(fileEntry, installPath);
                }
                File.Copy(filePath, installPath, true);

                // Setting manifest values
                if (fileEntry == null)
                {
                    fileEntry = this.GameManifest.GetFile(gamePath, true);
                }
                fileEntry.DeployMode = deployMode;
                fileEntry.SizeRaw = (int)fileToInstall.Length;
                fileEntry.SizeCompressed = fileEntry.SizeRaw;
                fileEntry.Version = LeagueInstallation.FantomeFilesVersion;
                this.HasChanged = true;
            }

            private void BackupFile(ReleaseManifest.ReleaseManifestFileEntry fileEntry, string filePath)
            {
                File.Copy(filePath, this.GetBackupPath(fileEntry), false);
            }

            private void RestoreFile(ReleaseManifest.ReleaseManifestFileEntry fileEntry, string filePath)
            {
                File.Copy(this.GetBackupPath(fileEntry), filePath, true);
            }

            private string GetBackupPath(ReleaseManifest.ReleaseManifestFileEntry fileEntry)
            {
                return String.Format("{0}lol-manager/backup/{1}/{2}/{3}", AppDomain.CurrentDomain.BaseDirectory, this.Project.Name, LeagueInstallation.GetReleaseString(fileEntry.Version), fileEntry.GetFullPath());
            }

            public void RevertFile(string gamePath)
            {
                if (this.OriginalManifest == null)
                {
                    throw new OriginalManifestNotLoadedException();
                }
                ReleaseManifest.ReleaseManifestFileEntry fileEntry = this.GameManifest.GetFile(gamePath, false);
                string installedPath = GetFileToInstallPath(gamePath, fileEntry.DeployMode, LeagueInstallation.FantomeFilesVersion);
                if (File.Exists(installedPath))
                {
                    File.Delete(installedPath);
                }
                ReleaseManifest.ReleaseManifestFileEntry originalEntry = this.OriginalManifest.GetFile(gamePath, false);
                if (originalEntry == null)
                {
                    // Installed file was a new file, remove it.
                    fileEntry.Remove();
                }
                else
                {
                    // Restore original file if necessary
                    if (originalEntry.DeployMode == ReleaseManifest.DeployMode.Deployed)
                    {
                        RestoreFile(originalEntry, installedPath);
                    }
                    // Revert original values
                    fileEntry.DeployMode = originalEntry.DeployMode;
                    fileEntry.LastWriteTime = originalEntry.LastWriteTime;
                    fileEntry.MD5 = originalEntry.MD5;
                    fileEntry.SizeCompressed = originalEntry.SizeCompressed;
                    fileEntry.SizeRaw = originalEntry.SizeRaw;
                    fileEntry.Version = originalEntry.Version;
                }
                this.HasChanged = true;
            }

            private string GetFileToInstallPath(string fileFullPath, ReleaseManifest.DeployMode deployMode, uint version)
            {
                if (deployMode == ReleaseManifest.DeployMode.Managed)
                {
                    return String.Format("{0}/managedfiles/{1}/{2}", this.Project.GetFolder(), LeagueInstallation.GetReleaseString(version), fileFullPath);
                }
                else if (deployMode == ReleaseManifest.DeployMode.Deployed)
                {
                    return String.Format("{0}/releases/{1}/deploy/{2}", this.Project.GetFolder(), this.Version, fileFullPath);
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

            public class OriginalManifestNotLoadedException : Exception
            {
                public OriginalManifestNotLoadedException() : base("The original manifest was not loaded.") { }
            }
        }
    }

}
