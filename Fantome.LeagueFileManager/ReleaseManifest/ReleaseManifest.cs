using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fantome.LeagueFileManager
{
    public partial class ReleaseManifest
    {
        public string FilePath { get; private set; }
        public short MajorVersion { get; private set; }
        public short MinorVersion { get; private set; }
        public uint ReleaseVersion { get; private set; }
        public string ProjectName { get; private set; }
        private List<string> Names = new List<string>();
        public ReleaseManifestFolderEntry Project { get; private set; }

        public ReleaseManifest(string filePath)
        {
            this.FilePath = filePath;
            using (BinaryReader br = new BinaryReader(File.OpenRead(filePath), Encoding.ASCII))
            {
                this.Read(br);
            }
        }

        private void Read(BinaryReader br)
        {
            string readMagic = Encoding.ASCII.GetString(br.ReadBytes(4));
            if (!String.Equals(readMagic, "RLSM"))
            {
                throw new InvalidMagicNumberException(readMagic);
            }
            this.MajorVersion = br.ReadInt16();
            this.MinorVersion = br.ReadInt16();

            int projectNameIndex = br.ReadInt32();
            this.ReleaseVersion = br.ReadUInt32();

            int folderCount = br.ReadInt32();

            List<ReleaseManifestFolderEntry> folders = new List<ReleaseManifestFolderEntry>();
            for (int i = 0; i < folderCount; i++)
            {
                folders.Add(new ReleaseManifestFolderEntry(br));
            }

            int fileCount = br.ReadInt32();

            List<ReleaseManifestFileEntry> files = new List<ReleaseManifestFileEntry>();
            for (int i = 0; i < fileCount; i++)
            {
                files.Add(new ReleaseManifestFileEntry(br));
            }

            int nameCount = br.ReadInt32();
            int nameSectionLength = br.ReadInt32();
            this.Names.AddRange(Encoding.ASCII.GetString(br.ReadBytes(nameSectionLength)).Split('\0'));
            this.Names.RemoveAt(this.Names.Count - 1);
            if (nameCount != this.Names.Count)
            {
                throw new InvalidNamesListException();
            }

            this.ProjectName = this.Names[projectNameIndex];

            // Assigning names and parent/sub entries to all file and folder entries
            foreach (ReleaseManifestFolderEntry folderEntry in folders)
            {
                folderEntry.Name = this.Names[folderEntry.NameIndex];
                for (int i = 0; i < folderEntry.SubFolderCount; i++)
                {
                    folders[folderEntry.SubFolderStartIndex + i].Parent = folderEntry;
                    folderEntry.Folders.Add(folders[folderEntry.SubFolderStartIndex + i]);
                }
                for (int i = 0; i < folderEntry.FileCount; i++)
                {
                    files[folderEntry.FileListStartIndex + i].Folder = folderEntry;
                    folderEntry.Files.Add(files[folderEntry.FileListStartIndex + i]);
                }
            }
            foreach (ReleaseManifestFileEntry fileEntry in files)
            {
                fileEntry.Name = this.Names[fileEntry.NameIndex];
            }
            this.Project = folders[0];
        }

        public ReleaseManifestFolderEntry GetFolder(string path, bool createIfNotFound)
        {
            if (path == "")
            {
                return this.Project;
            }
            string[] folders = path.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            ReleaseManifestFolderEntry baseFolder = this.Project;
            for (int i = 0; i < folders.Length; i++)
            {
                ReleaseManifestFolderEntry foundSubFolder = baseFolder.Folders.Find(x => String.Equals(x.Name, folders[i], StringComparison.InvariantCultureIgnoreCase));
                if (foundSubFolder == null)
                {
                    if (createIfNotFound)
                    {
                        ReleaseManifestFolderEntry newFolderEntry = new ReleaseManifestFolderEntry(folders[i], this.GetNameIndex(folders[i]), baseFolder);
                        baseFolder.Folders.Add(newFolderEntry);
                        baseFolder = newFolderEntry;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    baseFolder = foundSubFolder;
                }
            }
            return baseFolder;
        }

        public ReleaseManifestFileEntry GetFile(string path, bool createIfNotFound)
        {
            string[] folders = path.Split('/');
            string folderPath = path.Substring(0, path.Length - folders[folders.Length - 1].Length);
            ReleaseManifestFolderEntry gotFolder = this.GetFolder(folderPath, createIfNotFound);
            if (gotFolder == null)
            {
                return null;
            }
            else
            {
                ReleaseManifestFileEntry foundFile = gotFolder.Files.Find(x => String.Equals(x.Name, folders[folders.Length - 1], StringComparison.InvariantCultureIgnoreCase));
                if (foundFile == null && createIfNotFound)
                {
                    foundFile = new ReleaseManifestFileEntry(folders[folders.Length - 1], this.GetNameIndex(folders[folders.Length - 1]), gotFolder);
                    gotFolder.Files.Add(foundFile);
                }
                return foundFile;
            }
        }

        private int GetNameIndex(string name)
        {
            int gotIndex = this.Names.IndexOf(name);
            if (gotIndex == -1)
            {
                gotIndex = this.Names.Count;
                this.Names.Add(name);
            }
            return gotIndex;
        }

        public void Save()
        {
            this.Save(this.FilePath);
        }

        public void Save(string filePath)
        {
            int folderCount = 1 + GetFolderCount(this.Project);
            int fileCount = GetFileCount(this.Project);
            int nameSectionLength = GetNameSectionLength();
            SetFolderIndexes(this.Project, 1 + this.Project.Folders.Count);
            SetFileIndexes(this.Project, this.Project.Files.Count);
            using (BinaryWriter bw = new BinaryWriter(new FileStream(filePath, FileMode.Create)))
            {
                bw.Write(Encoding.ASCII.GetBytes("RLSM"));
                bw.Write(this.MajorVersion);
                bw.Write(this.MinorVersion);
                bw.Write(this.Names.IndexOf(this.ProjectName));
                bw.Write(this.ReleaseVersion);
                bw.Write(folderCount);
                this.Project.Write(bw);
                WriteSubFolderEntries(this.Project, bw);
                bw.Write(fileCount);
                WriteFileEntries(this.Project, bw);
                bw.Write(this.Names.Count);
                bw.Write(nameSectionLength);
                foreach (string name in this.Names)
                {
                    bw.Write(Encoding.ASCII.GetBytes(name));
                    bw.Write((byte)0);
                }
            }
        }

        private static int GetFolderCount(ReleaseManifestFolderEntry folderEntry)
        {
            int folderCount = folderEntry.Folders.Count;
            foreach (ReleaseManifestFolderEntry subFolderEntry in folderEntry.Folders)
            {
                folderCount += GetFolderCount(subFolderEntry);
            }
            return folderCount;
        }
        private static int GetFileCount(ReleaseManifestFolderEntry folderEntry)
        {
            int fileCount = folderEntry.Files.Count;
            foreach (ReleaseManifestFolderEntry subFolderEntry in folderEntry.Folders)
            {
                fileCount += GetFileCount(subFolderEntry);
            }
            return fileCount;
        }
        private static int SetFolderIndexes(ReleaseManifestFolderEntry baseFolder, int index)
        {
            foreach (ReleaseManifestFolderEntry subFolderEntry in baseFolder.Folders)
            {
                subFolderEntry.SubFolderStartIndex = index;
                index = SetFolderIndexes(subFolderEntry, index + subFolderEntry.Folders.Count);
            }
            return index;
        }
        private static int SetFileIndexes(ReleaseManifestFolderEntry baseFolder, int index)
        {
            foreach (ReleaseManifestFolderEntry subFolderEntry in baseFolder.Folders)
            {
                subFolderEntry.FileListStartIndex = index;
                index = SetFileIndexes(subFolderEntry, index + subFolderEntry.Files.Count);
            }
            return index;
        }
        private int GetNameSectionLength()
        {
            int length = 0;
            foreach (string name in this.Names)
            {
                length += 1 + name.Length;
            }
            return length;
        }
        private static void WriteSubFolderEntries(ReleaseManifestFolderEntry baseFolder, BinaryWriter bw)
        {
            foreach (ReleaseManifestFolderEntry folderEntry in baseFolder.Folders)
            {
                folderEntry.Write(bw);
            }
            foreach (ReleaseManifestFolderEntry folderEntry in baseFolder.Folders)
            {
                WriteSubFolderEntries(folderEntry, bw);
            }
        }
        private static void WriteFileEntries(ReleaseManifestFolderEntry baseFolder, BinaryWriter bw)
        {
            foreach (ReleaseManifestFileEntry fileEntry in baseFolder.Files)
            {
                fileEntry.Write(bw);
            }
            foreach (ReleaseManifestFolderEntry folderEntry in baseFolder.Folders)
            {
                WriteFileEntries(folderEntry, bw);
            }
        }

        public enum DeployMode : uint
        {
            Deployed = 0,
            SolutionDeployed = 4,
            Managed = 5,
            RAFRaw = 6,
            RAFCompressed = 22
        }

        public class InvalidMagicNumberException : Exception
        {
            public InvalidMagicNumberException(string readMagic) : base(String.Format("Invalid magic number (\"{0}\"), expected: \"RLSM\".", readMagic)) { }
        }

        public class InvalidNamesListException : Exception
        {
            public InvalidNamesListException() : base("Names counts don't match.") { }
        }
    }
}
