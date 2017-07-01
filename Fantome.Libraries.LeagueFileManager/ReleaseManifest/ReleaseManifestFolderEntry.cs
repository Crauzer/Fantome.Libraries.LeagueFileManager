using System.Collections.Generic;
using System.IO;

namespace Fantome.Libraries.LeagueFileManager
{
    public partial class ReleaseManifest
    {
        public class ReleaseManifestFolderEntry
        {
            public string Name { get; set; }
            public int NameIndex { get; private set; }
            public int SubFolderStartIndex { get; set; }
            public int SubFolderCount { get; private set; }
            public int FileListStartIndex { get; set; }
            public int FileCount { get; private set; }
            public ReleaseManifestFolderEntry Parent { get; set; }
            public List<ReleaseManifestFolderEntry> Folders { get; private set; } = new List<ReleaseManifestFolderEntry>();
            public List<ReleaseManifestFileEntry> Files { get; private set; } = new List<ReleaseManifestFileEntry>();

            public ReleaseManifestFolderEntry(BinaryReader br)
            {
                this.NameIndex = br.ReadInt32();
                this.SubFolderStartIndex = br.ReadInt32();
                this.SubFolderCount = br.ReadInt32();
                this.FileListStartIndex = br.ReadInt32();
                this.FileCount = br.ReadInt32();
            }

            public ReleaseManifestFolderEntry(string name, int nameIndex, ReleaseManifestFolderEntry parent)
            {
                this.Name = name;
                this.NameIndex = nameIndex;
                this.Parent = parent;
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(this.NameIndex);
                bw.Write(this.SubFolderStartIndex);
                bw.Write(this.Folders.Count);
                bw.Write(this.FileListStartIndex);
                bw.Write(this.Files.Count);
            }

            public string GetFullPath()
            {
                if (this.Parent?.Parent != null)
                {
                    return this.Parent.GetFullPath() + "/" + this.Name;
                }
                else
                {
                    return this.Name;
                }
            }

            public void Remove()
            {
                if (this.Parent.Folders.Contains(this))
                {
                    this.Parent.Folders.Remove(this);
                }
            }
        }
    }
}
