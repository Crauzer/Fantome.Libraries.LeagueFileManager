﻿using System;
using System.IO;
using Fantome.Libraries.LeagueFileManager.ReleaseManifest;
using System.Linq;
using System.Security.Cryptography;

namespace Fantome.Libraries.LeagueFileManager
{
    internal class LeagueRADSProjectRelease
    {
        public readonly LeagueRADSProject Project;
        public readonly string Version;
        public readonly uint VersionValue;
        public ReleaseManifestFile GameManifest { get; private set; }
        public ReleaseManifestFile OriginalManifest { get; private set; }
        public bool HasChanged { get; private set; }

        public LeagueRADSProjectRelease(LeagueRADSProject project, string version)
        {
            this.Project = project;
            this.Version = version;
            this.VersionValue = LeagueRADSInstallation.GetReleaseValue(version);
            string manifestPath = this.GetFolder() + "/releasemanifest";
            if (File.Exists(manifestPath))
            {
                this.GameManifest = new ReleaseManifestFile(manifestPath);
            }
            else
            {
                throw new ReleaseManifestNotFoundException();
            }
        }

        public void LoadOriginalManifest()
        {
            string originalManifestFolder = String.Format("{0}/{1}/manifests/{2}",
                this.Project.Installation.ManagerInstallationFolder,
                this.Project.Name,
                this.Version);
            string manifestPath = originalManifestFolder + "/releasemanifest";
            Directory.CreateDirectory(originalManifestFolder);
            if (!File.Exists(manifestPath))
            {
                File.Copy(this.GameManifest.FilePath, manifestPath);
            }
            this.OriginalManifest = new ReleaseManifestFile(manifestPath);
        }

        public string GetFolder()
        {
            return String.Format("{0}/releases/{1}", this.Project.GetFolder(), this.Version);
        }

        public void InstallFile(ModifiedFile modifiedFile, LeagueRADSDeployRules deployRules)
        {
            if (modifiedFile.FilePath == null)
                throw new NotSpecifiedFileToInstallException();

            if (modifiedFile.MD5 == null)
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(modifiedFile.FilePath))
                    {
                        modifiedFile.MD5 = md5.ComputeHash(stream);
                    }
                }
            }

            FileInfo fileToInstall = new FileInfo(modifiedFile.FilePath);
            if (!fileToInstall.Exists)
                throw new FileToInstallNotFoundException();

            // Getting the matching file entry (null if new file)
            ReleaseManifestFileEntry fileEntry = this.GameManifest.GetFile(modifiedFile.GamePath, false);

            // File is already installed, don't install it again
            if (fileEntry != null && fileEntry.MD5.SequenceEqual(modifiedFile.MD5))
                return;

            // Finding the deploy mode to use
            ReleaseManifestFile.DeployMode deployMode = deployRules.GetTargetDeployMode(this.Project.Name, fileEntry);

            // Installing file
            string installPath = this.GetFileToInstallPath(modifiedFile.GamePath, deployMode, LeagueRADSInstallation.FantomeFilesVersion);
            Directory.CreateDirectory(Path.GetDirectoryName(installPath));
            if (fileEntry != null && (deployMode == ReleaseManifestFile.DeployMode.Deployed4 || deployMode == ReleaseManifestFile.DeployMode.Deployed0))
            {
                // Backup deployed file
                BackupFile(fileEntry, installPath);
            }
            File.Copy(modifiedFile.FilePath, installPath, true);

            // Setting manifest values
            if (fileEntry == null)
            {
                fileEntry = this.GameManifest.GetFile(modifiedFile.GamePath, true);
            }
            fileEntry.MD5 = modifiedFile.MD5;
            fileEntry.DeployMode = deployMode;
            fileEntry.SizeRaw = (int)fileToInstall.Length;
            fileEntry.SizeCompressed = fileEntry.SizeRaw;
            fileEntry.Version = LeagueRADSInstallation.FantomeFilesVersion;
            this.HasChanged = true;
        }

        private void BackupFile(ReleaseManifestFileEntry fileEntry, string filePath)
        {
            File.Copy(filePath, this.GetBackupPath(fileEntry), false);
        }

        private void RestoreFile(ReleaseManifestFileEntry fileEntry, string filePath)
        {
            File.Copy(this.GetBackupPath(fileEntry), filePath, true);
        }

        private string GetBackupPath(ReleaseManifestFileEntry fileEntry)
        {
            return String.Format("{0}/{1}/backup/{2}/{3}", this.Project.Installation.ManagerInstallationFolder, this.Project.Name, LeagueRADSInstallation.GetReleaseString(fileEntry.Version), fileEntry.GetFullPath());
        }

        public void RevertFile(ModifiedFile modifiedFile)
        {
            if (this.OriginalManifest == null)
            {
                throw new OriginalManifestNotLoadedException();
            }
            ReleaseManifestFileEntry fileEntry = this.GameManifest.GetFile(modifiedFile.GamePath, false);
            string installedPath = GetFileToInstallPath(modifiedFile.GamePath, fileEntry.DeployMode, fileEntry.Version);
            if (File.Exists(installedPath))
            {
                File.Delete(installedPath);
            }
            ReleaseManifestFileEntry originalEntry = this.OriginalManifest.GetFile(modifiedFile.GamePath, false);
            if (originalEntry == null)
            {
                // Installed file was a new file, remove it.
                fileEntry.Remove();
            }
            else
            {
                // Restore original file if necessary
                if (originalEntry.DeployMode == ReleaseManifestFile.DeployMode.Deployed4 || originalEntry.DeployMode == ReleaseManifestFile.DeployMode.Deployed0)
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

        private string GetFileToInstallPath(string fileFullPath, ReleaseManifestFile.DeployMode deployMode, uint version)
        {
            if (deployMode == ReleaseManifestFile.DeployMode.Managed)
            {
                return String.Format("{0}/managedfiles/{1}/{2}", this.Project.GetFolder(), LeagueRADSInstallation.GetReleaseString(version), fileFullPath);
            }
            else if (deployMode == ReleaseManifestFile.DeployMode.Deployed4 || deployMode == ReleaseManifestFile.DeployMode.Deployed0)
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

        public class NotSpecifiedFileToInstallException : Exception
        {
            public NotSpecifiedFileToInstallException() : base("The file to install path cannot be null.") { }
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