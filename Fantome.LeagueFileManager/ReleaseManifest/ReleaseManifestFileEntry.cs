using System.IO;
using System;

namespace Fantome.LeagueFileManager
{
    public partial class ReleaseManifest
    {
        public class ReleaseManifestFileEntry
        {
            public string Name { get; set; }
            public int NameIndex { get; private set; }
            public uint Version { get; set; }
            private byte[] _MD5;
            public byte[] MD5
            {
                get { return _MD5; }
                set
                {
                    if (value?.Length == 16)
                    {
                        if (_MD5 == null)
                        {
                            _MD5 = value;
                        }
                        else
                        {
                            for (int i = 0; i < 16; i++)
                            {
                                _MD5[i] = value[i];
                            }
                        }

                    }
                }
            }
            public DeployMode DeployMode { get; set; }
            public int SizeRaw { get; set; }
            public int SizeCompressed { get; set; }
            public DateTime LastWriteTime { get; set; }
            public ReleaseManifestFolderEntry Folder { get; set; }

            public ReleaseManifestFileEntry(BinaryReader br)
            {
                this.NameIndex = br.ReadInt32();
                this.Version = br.ReadUInt32();
                this.MD5 = br.ReadBytes(16);
                this.DeployMode = (DeployMode)br.ReadUInt32();
                this.SizeRaw = br.ReadInt32();
                this.SizeCompressed = br.ReadInt32();
                this.LastWriteTime = DateTime.FromBinary(br.ReadInt64()).AddYears(1600);
            }

            public ReleaseManifestFileEntry(string name, int nameIndex, ReleaseManifestFolderEntry folder)
            {
                this.Name = name;
                this.NameIndex = nameIndex;
                this.MD5 = new byte[16];
                this.Folder = folder;
                this.LastWriteTime = new DateTime();
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(this.NameIndex);
                bw.Write(this.Version);
                bw.Write(this.MD5);
                bw.Write((uint)this.DeployMode);
                bw.Write(this.SizeRaw);
                bw.Write(this.SizeCompressed);
                bw.Write(this.LastWriteTime.AddYears(-1600).ToBinary());
            }
        }
    }
}
